using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IAuthService
{
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
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<AuthService> _logger;
    private readonly bool _requireEmailVerification;

    public AuthService(
        AppDbContext db,
        IJwtTokenService jwtTokenService,
        IPasswordHashService passwordHashService,
        IRefreshTokenService refreshTokenService,
        IAuditLogService auditLog,
        ILogger<AuthService> logger,
        IConfiguration configuration)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordHashService = passwordHashService;
        _refreshTokenService = refreshTokenService;
        _auditLog = auditLog;
        _logger = logger;
        _requireEmailVerification = bool.Parse(
            configuration["Auth:RequireEmailVerification"] ?? "false");
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return FailToken("Username and password are required");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !user.IsActive)
        {
            await _auditLog.LogAsync(
                SecurityEvent.LoginFailed, false, ipAddress,
                username: request.Username, details: "user_not_found");

            return FailToken("Invalid credentials");
        }

        if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditLog.LogAsync(
                SecurityEvent.LoginFailed, false, ipAddress,
                userId: user.Id, username: user.Username, details: "invalid_password");

            return FailToken("Invalid credentials");
        }

        if (_requireEmailVerification && !user.IsEmailVerified)
        {
            await _auditLog.LogAsync(
                SecurityEvent.LoginFailed, false, ipAddress,
                userId: user.Id, username: user.Username, details: "email_not_verified");

            return FailToken("Email not verified. Check your inbox.");
        }

        var accessToken = _jwtTokenService.GenerateToken(user);
        var (_, rawRefreshToken) = await _refreshTokenService.CreateAsync(user.Id, ipAddress);

        await _auditLog.LogAsync(
            SecurityEvent.LoginSuccess, true, ipAddress,
            userId: user.Id, username: user.Username);

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

            await _auditLog.LogAsync(
                SecurityEvent.TokenReuseDetected, false, ipAddress,
                userId: stored.UserId, details: $"family={stored.Family}");

            return FailToken("Token reuse detected. All sessions have been revoked");
        }

        if (stored.IsExpired)
        {
            _logger.LogInformation(
                "Refresh token expired for UserId={UserId} IP={IpAddress}",
                stored.UserId, ipAddress);

            return FailToken("Refresh token has expired");
        }

        var user = await _db.Users.FindAsync(stored.UserId);

        if (user == null || !user.IsActive)
            return FailToken("User not found or inactive");

        var (_, newRawToken) = await _refreshTokenService.RotateAsync(stored, ipAddress);
        var accessToken = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation(
            "Token rotated for UserId={UserId} IP={IpAddress}",
            user.Id, ipAddress);

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

        await _auditLog.LogAsync(
            SecurityEvent.TokenRevoked, true, ipAddress,
            userId: stored.UserId);

        return true;
    }

    public async Task<User?> GetUserByIdAsync(int id)
        => await _db.Users.FindAsync(id);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    private static TokenResponse FailToken(string message)
        => new() { Success = false, Message = message };
}
