using System.Text.RegularExpressions;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<TokenResponse> RefreshAsync(string rawRefreshToken, string ipAddress);
    Task<bool> RevokeAsync(string rawRefreshToken, string ipAddress);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IRefreshTokenService _refreshTokenService;

    private static readonly Regex EmailValidation = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IPasswordHashService passwordHashService,
        IRefreshTokenService refreshTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordHashService = passwordHashService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Fail("Username, email and password are required");
        }

        if (request.Password != request.ConfirmPassword)
            return Fail("Passwords do not match");

        if (request.Password.Length < 8)
            return Fail("Password must be at least 8 characters");

        if (!EmailValidation.IsMatch(request.Email))
            return Fail("Invalid email format");

        if (request.Username.Length < 3 || request.Username.Length > 50)
            return Fail("Username must be between 3 and 50 characters");

        var usernameTaken = await _db.Users.AnyAsync(u =>
            u.Username == request.Username.Trim());

        if (usernameTaken)
            return Fail("Username already exists");

        var emailTaken = await _db.Users.AnyAsync(u =>
            u.Email == request.Email.Trim().ToLower());

        if (emailTaken)
            return Fail("Email already registered");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = _passwordHashService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "User registered successfully",
            User = ToDto(user)
        };
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return FailToken("Username and password are required");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Username);

        if (user == null || !user.IsActive || !_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            return FailToken("Invalid credentials");

        var accessToken = _jwtTokenService.GenerateToken(user);
        var (_, rawRefreshToken) = await _refreshTokenService.CreateAsync(user.Id, ipAddress);

        return new TokenResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresIn = _jwtTokenService.ExpirationSeconds
        };
    }

    public async Task<TokenResponse> RefreshAsync(string rawRefreshToken, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            return FailToken("Refresh token is required");

        var stored = await _refreshTokenService.FindByRawTokenAsync(rawRefreshToken);

        if (stored == null)
            return FailToken("Invalid token");

        if (stored.IsRevoked)
        {
            await _refreshTokenService.RevokeFamilyAsync(stored.Family, ipAddress);
            return FailToken("Token reuse detected. All sessions have been revoked");
        }

        if (stored.IsExpired)
            return FailToken("Refresh token has expired");

        var user = await _db.Users.FindAsync(stored.UserId);

        if (user == null || !user.IsActive)
            return FailToken("User not found or inactive");

        var (_, newRawToken) = await _refreshTokenService.RotateAsync(stored, ipAddress);
        var accessToken = _jwtTokenService.GenerateToken(user);

        return new TokenResponse
        {
            Success = true,
            Message = "Token refreshed",
            AccessToken = accessToken,
            RefreshToken = newRawToken,
            ExpiresIn = _jwtTokenService.ExpirationSeconds
        };
    }

    public async Task<bool> RevokeAsync(string rawRefreshToken, string ipAddress)
    {
        var stored = await _refreshTokenService.FindByRawTokenAsync(rawRefreshToken);

        if (stored == null || stored.IsRevoked)
            return false;

        await _refreshTokenService.RevokeAsync(stored, ipAddress);
        return true;
    }

    public async Task<User?> GetUserByIdAsync(int id)
        => await _db.Users.FindAsync(id);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    private static AuthResponse Fail(string message)
        => new() { Success = false, Message = message };

    private static TokenResponse FailToken(string message)
        => new() { Success = false, Message = message };

    private static UserDto ToDto(User user)
        => new() { Id = user.Id, Username = user.Username, Email = user.Email };
}
