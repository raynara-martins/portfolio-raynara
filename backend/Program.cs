using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PortfolioApi.Data;
using PortfolioApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================
// JWT
// =====================
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"] ?? "CHANGE_ME_SUPER_SECRET_KEY_123";
var keyBytes = Encoding.ASCII.GetBytes(secret);

// =====================
// Services
// =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .WithOrigins("http://localhost:3000"));
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtSection["Audience"],
            ValidIssuer = jwtSection["Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            // opcional: remover tolerância de clock
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger + Bearer (modo HTTP -> não precisa digitar "Bearer ")
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PortfolioApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Cole apenas o token JWT (sem a palavra 'Bearer')."
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

var app = builder.Build();

// =====================
// Pipeline
// =====================
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

// =====================
// Endpoints
// =====================

// Login (sem hash, será ajustado no Dia 4)
app.MapPost("/login", async (AppDbContext db, LoginDto dto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user is null) return Results.Unauthorized();
    if (user.Password != dto.Password) return Results.Unauthorized();

    var token = JwtGenerator.GenerateToken(
        user,
        issuer: jwtSection["Issuer"] ?? "PortfolioApi",
        audience: jwtSection["Audience"] ?? "PortfolioFrontend",
        keyBytes
    );

    return Results.Ok(new
    {
        token,
        user = new { user.Name, user.Email }
    });
});

// /me (precisa de Bearer Token)
app.MapGet("/me", async (AppDbContext db, HttpContext http) =>
{
    var email = http.User?.Identity?.Name;
    if (string.IsNullOrEmpty(email)) return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    return user is null
        ? Results.NotFound()
        : Results.Ok(new { user.Name, user.Email });
}).RequireAuthorization();

// /certificados (público no Dia 3)
app.MapGet("/certificados", async (AppDbContext db) =>
{
    var list = await db.Certificates.OrderBy(c => c.Id).ToListAsync();
    return Results.Ok(list);
});

app.Run();

// =====================
// Tipos auxiliares
// =====================
public record LoginDto(string Email, string Password);

public static class JwtGenerator
{
    public static string GenerateToken(User user, string issuer, string audience, byte[] key)
    {
        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
