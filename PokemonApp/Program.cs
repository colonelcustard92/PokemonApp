using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PokemonApp.Models;
using PokemonApp.RequestModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokemonApp.Services;
using Microsoft.OpenApi.Models;
using PokemonApp.DomainModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("https://localhost:5173") // Vue dev server URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



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
        Description = "Paste your JWT Key here"
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

builder.Services.AddDbContext<PokemonDbContext>(options =>
    options.UseSqlite("Data Source=pokedex.db"));


var app = builder.Build();
app.UseCors("AllowVueApp");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var randomClaims = new List<Claim>
        {
            new(ClaimTypes.Name, "testUser"),
            new(ClaimTypes.Email, "testuser@example.com"),
            new(ClaimTypes.Role, "User"),
            new("CustomClaim1", "Value1"),
            new("CustomClaim2", "Value2")
        };

        db.Users.Add(new User
        {
            Username = "test",
            PasswordHash = "test123",
            Id = new Random().Next(500), //I'd use a GUID in production to guarantee uniqueness - Random() isn't truly Random after all :)
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
        using var context = new PokemonDbContext();

        // Find the user
        var user = context.Users.FirstOrDefault(u => u.Username == request.Username);
        if (user is null) return Results.NotFound("User not found");

        // Plaintext password check (OK for coding test)
        if (user.PasswordHash != request.Password) return Results.Unauthorized();

        // Create JWT token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { Token = tokenString });

    })
    .WithName("Login")
    .Produces(200)
    .Produces(401)
    .WithOpenApi();

app.MapGet("/", async ([FromQuery] string? name, [FromQuery] int? id) =>
{
    try
    {
        var pokeServe = new PokemonServices(new HttpClient());
        var pokemonReturnedFromApi = await pokeServe.GetPokemon(new PokemonRequestModel { Name = name, Id = id });
        if (pokemonReturnedFromApi == new PokemonResponseModel()) return Results.NotFound(new { Error = "Pokemon not found. Please check the name or ID." });

        return Results.Ok(new PokemonResponseModel
        {
            Name = pokemonReturnedFromApi.Name,
            Sprites = pokemonReturnedFromApi.Sprites,
            Height = pokemonReturnedFromApi.Height,
            Weight = pokemonReturnedFromApi.Weight,
            Abilities = pokemonReturnedFromApi.Abilities,
            Types = pokemonReturnedFromApi.Types
        });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { Error = "An unexpected error occurred.", Details = ex.Message, ex.StackTrace },
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
