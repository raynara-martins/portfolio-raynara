using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioApi.DTOs.Auth;              // LoginRequest
using PortfolioApi.Services.Interfaces;    // IAuthService

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Faz login e retorna o token + dados básicos do usuário.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // o serviço retorna (bool ok, string? token, User? user)
        var (ok, token, user) = await _auth.LoginAsync(dto.Email, dto.Password);

        if (!ok || user is null || string.IsNullOrWhiteSpace(token))
            return Unauthorized();

        return Ok(new
        {
            token,
            user = new { user.Name, user.Email }
        });
    }
}
