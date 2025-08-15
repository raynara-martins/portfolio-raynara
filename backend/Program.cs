using System.Text;
using System.Security.Claims; // <- ADICIONE ISSO
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PortfolioApi.Data;
using PortfolioApi.Repositories;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services;
using PortfolioApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// DB por ambiente
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("tests-db"));
}
else
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));
}

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .WithOrigins("http://localhost:3000"));
});

// JWT (≥ 32 chars)
var issuer   = builder.Configuration["Jwt:Issuer"]   ?? "PortfolioApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "PortfolioFrontend";
var secret   = builder.Configuration["Jwt:Secret"]   ?? "default-test-secret-32-chars-minimum-__";
if (secret.Length < 32) secret = secret.PadRight(32, '_');
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Controllers + Swagger
builder.Services.AddControllers();
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
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// para WebApplicationFactory nos testes
public partial class Program { }
