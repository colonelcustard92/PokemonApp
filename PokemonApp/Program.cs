using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PokemonApp.Models;
using PokemonApp.RequestModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PokemonApp.Services;
using Microsoft.OpenApi.Models;
using PokemonApp.DomainModels;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var jwtKey = "2YvWz9yU7R1V9mNefYos0CRmXlj8T4qfZRaCzNWo6m8=\r\n"; // Store securely

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var randomClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("CustomClaim1", "Value1"),
            new Claim("CustomClaim2", "Value2")
        };

        db.Users.Add(new User
        {
            Username = "test",
            PasswordHash = "test123",
            Id = new Random().Next(500),
            Claims = randomClaims.Select(c => new UserClaim { Type = c.Type, Value = c.Value }).ToList()
        });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", ([FromBody] LoginRequestModel request) =>
{
    if (request.Username == "admin" && request.Password == "password") // Simplified for testing
    {
        //Generate Claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        // Create JWT Token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //Assign Claims to the Token
        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );
        //Return the Token
        return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    //If they got this far, they are unauthorized
    return Results.Unauthorized();
}).WithName("Login")
  .Produces(200)
  .Produces(401)
  .WithOpenApi();

app.MapGet("/", async ([FromQuery] string? name, [FromQuery] int? ID) =>
{
    try
    {
       
        var pokeServe = new PokemonServices(new HttpClient());
        var pokemonReturnedFromAPI = await pokeServe.GetPokemon(new PokemonRequestModel { Name = name, ID = ID });
        if (pokemonReturnedFromAPI == new PokemonResponseModel()) return Results.NotFound(new { Error = "Pokemon not found. Please check the name or ID." });

        return Results.Ok(new PokemonResponseModel
        {
            Name = pokemonReturnedFromAPI.Name,
            Sprites = pokemonReturnedFromAPI.Sprites,
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
    }
})
.RequireAuthorization()
.WithName("GetPokemon")
.Produces<PokemonResponseModel>(200, "application/json")
.Produces(400)
.Produces(401)
.Produces(404)
.Produces(500)
.WithOpenApi();

app.Run();
