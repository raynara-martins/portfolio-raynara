using PortfolioApi.Models;

namespace PortfolioApi.Repositories.Interfaces;

public interface ICertificateRepository
{
    Task<List<Certificate>> GetByUserIdAsync(int userId);
}
