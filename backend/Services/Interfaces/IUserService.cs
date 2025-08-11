using PortfolioApi.Models;

namespace PortfolioApi.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
}
