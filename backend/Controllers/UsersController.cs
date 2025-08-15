using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var email = User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var user = await _users.GetByEmailAsync(email);
        if (user is null) return NotFound();

        return Ok(new { user.Name, user.Email });
    }
}
