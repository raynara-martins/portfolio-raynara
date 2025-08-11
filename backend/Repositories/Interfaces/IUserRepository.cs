using PortfolioApi.Models;

namespace PortfolioApi.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}
