using Microsoft.EntityFrameworkCore;
using PortfolioApi.Data;
using PortfolioApi.Models;
using PortfolioApi.Repositories.Interfaces;

namespace PortfolioApi.Repositories;

public class CertificateRepository(AppDbContext db) : ICertificateRepository
{
    public Task<List<Certificate>> GetByUserIdAsync(int userId) =>
        db.Certificates
          .AsNoTracking()
          .Where(c => c.User_Id == userId)
          .OrderBy(c => c.Id)
          .ToListAsync();
}
