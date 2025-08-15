// backend/Services/Interfaces/IAuthService.cs
using PortfolioApi.Models;

namespace PortfolioApi.Services.Interfaces;

public interface IAuthService
{
    Task<(bool ok, string? token, User? user)> LoginAsync(string email, string passwordPlain);
}
