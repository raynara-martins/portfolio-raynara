using PortfolioApi.Models;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Services;

public class UserService(IUserRepository users) : IUserService
{
    public Task<User?> GetByEmailAsync(string email) => users.GetByEmailAsync(email);
}
