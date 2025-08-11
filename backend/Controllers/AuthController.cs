using Microsoft.AspNetCore.Mvc;
using PortfolioApi.DTOs.Auth;
using PortfolioApi.Services.Interfaces;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await authService.LoginAsync(req.Email, req.Password);
        if (!result.ok) return Unauthorized();

        return Ok(new
        {
            token = result.token,
            user = new { result.user!.Name, result.user.Email }
        });
    }
}
