using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Success = false, Message = "Invalid input" });

        var response = await _authService.RegisterAsync(request);
        
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Success = false, Message = "Invalid input" });

        var response = await _authService.LoginAsync(request);
        
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrEmpty(usernameClaim))
            return Unauthorized(new { message = "User not authenticated" });

        var user = await _authService.GetUserByUsernameAsync(usernameClaim);
        
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Email = user.Email 
        });
    }
}
