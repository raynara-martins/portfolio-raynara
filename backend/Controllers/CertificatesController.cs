using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CertificatesController(IUserService userService, ICertificateRepository certRepo) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var email = User?.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var u = await userService.GetByEmailAsync(email);
        if (u is null) return NotFound();

        var list = await certRepo.GetByUserIdAsync(u.Id);
        return Ok(list);
    }
}
