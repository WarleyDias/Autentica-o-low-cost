using Microsoft.AspNetCore.Mvc;
using AuthSystem.Services;
using AuthSystem.Models;

namespace AuthSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrEmpty(usernameClaim))
            return Unauthorized(new { message = "Usuário não autenticado" });

        var user = await _authService.GetUserByUsernameAsync(usernameClaim);
        
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        return Ok(new UserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Email = user.Email 
        });
    }
}
