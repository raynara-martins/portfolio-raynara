using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var email = User?.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var u = await userService.GetByEmailAsync(email);
        if (u is null) return NotFound();

        return Ok(new { u.Name, u.Email });
    }
}
