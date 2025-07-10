using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PokemonApp.Models;
using PokemonApp.DTOs;  
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PokemonApp.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = "2YvWz9yU7R1V9mNefYos0CRmXlj8T4qfZRaCzNWo6m8=\r\n"; // This should be stored securely, e.g., in Azure Key Vault or environment variables

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Add JWT Bearer token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT in the format: Bearer {your token here}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", ([FromBody] LoginDTO request) =>
{
    if (request.Username == "admin" && request.Password == "password") // This is simplified for testing purposes - in a real application, you would validate against a database or other secure store such as Azure Entra ID
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    return Results.Unauthorized();
}).WithName("Login")
  .Produces(200)
  .Produces(401)
  .WithOpenApi();
  
app.MapGet("/", async ([FromQuery] string? name,[FromQuery] int? ID) =>
{
    try
    {
       var pokeServe = new PokemonServices(new HttpClient());
        var pokemonReturnedFromAPI = await pokeServe.GetPokemon( new PokemonDTO { Name = name, ID = ID });
        return Results.Ok(new Pokemon
        {
            Name = pokemonReturnedFromAPI.Name,
            Sprite = pokemonReturnedFromAPI.Sprite,
            Height = pokemonReturnedFromAPI.Height,
            Weight = pokemonReturnedFromAPI.Weight,
            Abilities = pokemonReturnedFromAPI.Abilities,
            Types = pokemonReturnedFromAPI.Types           
        });
    }

    catch (Exception ex)
    {
        return Results.Json(
            new { Error = "An unexpected error occurred.", Details = ex.Message, StackTrace = ex.StackTrace },
            statusCode: 500
        );        
        // Log to App insights or similar
    }
})
.RequireAuthorization()
.WithName("GetPokemon")
.Produces<Pokemon>(200, "application/json")
.Produces(400)
.WithOpenApi();


app.Run();





