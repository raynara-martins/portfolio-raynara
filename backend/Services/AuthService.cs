using PortfolioApi.Models;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services.Interfaces;
using PortfolioApi.Utils;

namespace PortfolioApi.Services;

public class AuthService(IUserRepository users, IConfiguration cfg) : IAuthService
{
    public async Task<(bool ok, string? token, User? user)> LoginAsync(string email, string passwordPlain)
    {
        var user = await users.GetByEmailAsync(email);
        if (user is null) return (false, null, null);

        var saved = user.Password ?? "";
        var looksHashed = saved.StartsWith("$2a$") || saved.StartsWith("$2b$") || saved.StartsWith("$2y$");

        bool passwordOk = looksHashed
            ? BCrypt.Net.BCrypt.Verify(passwordPlain, saved)
            : saved == passwordPlain;

        if (!passwordOk) return (false, null, null);

        var token = JwtGenerator.GenerateToken(
            user,
            issuer: cfg["Jwt:Issuer"] ?? "PortfolioApi",
            audience: cfg["Jwt:Audience"] ?? "PortfolioFrontend",
            secret: cfg["Jwt:Secret"] ?? "CHANGE_ME_SUPER_SECRET_KEY_123"
        );

        return (true, token, user);
    }
}
