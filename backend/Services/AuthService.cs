using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PortfolioApi.Models;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    public async Task<(bool ok, string? token, User? user)> LoginAsync(string email, string passwordPlain)
    {
        var user = await _users.GetByEmailAsync(email);
        if (user is null) return (false, null, null);

        var valid = user.Password?.StartsWith("$2") == true
            ? BCrypt.Net.BCrypt.Verify(passwordPlain, user.Password)
            : string.Equals(user.Password, passwordPlain);

        if (!valid) return (false, null, null);

        var token = GenerateJwt(user);
        return (true, token, user);
    }

    private string GenerateJwt(User user)
    {
        var issuer  = _config["Jwt:Issuer"]   ?? "PortfolioApi";
        var audience= _config["Jwt:Audience"] ?? "PortfolioFrontend";

        // **Precisa ter 32+ caracteres**
        var secret  = _config["Jwt:Secret"] ?? "CHANGE_ME_SUPER_SECRET_KEY_1234567890_32+";
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        // Claim que o /me usa (User.Identity.Name)
        var email = user.Email ?? string.Empty;

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, email),
            // opcionais (bom pra debug/auditoria)
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email)
        });

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
