using System.Text.RegularExpressions;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IAccountService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, string ipAddress);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request, string ipAddress);
    Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request, string ipAddress);
}

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUserTokenService _userTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLog;

    private static readonly Regex EmailValidation = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    public AccountService(
        AppDbContext db,
        IPasswordHashService passwordHashService,
        IUserTokenService userTokenService,
        IRefreshTokenService refreshTokenService,
        IEmailService emailService,
        IAuditLogService auditLog)
    {
        _db = db;
        _passwordHashService = passwordHashService;
        _userTokenService = userTokenService;
        _refreshTokenService = refreshTokenService;
        _emailService = emailService;
        _auditLog = auditLog;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
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

        if (await _db.Users.AnyAsync(u => u.Username == request.Username.Trim()))
            return Fail("Username already exists");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLower()))
            return Fail("Email already registered");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = _passwordHashService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = false
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var verificationToken = await _userTokenService.CreateAsync(user.Id, UserTokenType.EmailVerification);
        await _emailService.SendEmailVerificationAsync(user.Email, user.Username, verificationToken);

        return new AuthResponse
        {
            Success = true,
            Message = "User registered. Check your email to verify your account.",
            User = new UserDto { Id = user.Id, Username = user.Username, Email = user.Email }
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return;

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email.Trim().ToLower() && u.IsActive);

        if (user == null)
        {
            await _auditLog.LogAsync(
                SecurityEvent.PasswordResetRequested, false, ipAddress,
                details: "email_not_found");
            return;
        }

        var rawToken = await _userTokenService.CreateAsync(user.Id, UserTokenType.PasswordReset);
        await _emailService.SendPasswordResetAsync(user.Email, user.Username, rawToken);

        await _auditLog.LogAsync(
            SecurityEvent.PasswordResetRequested, true, ipAddress,
            userId: user.Id, username: user.Username);
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Fail("Token is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return Fail("New password is required");

        if (request.NewPassword != request.ConfirmNewPassword)
            return Fail("Passwords do not match");

        if (request.NewPassword.Length < 8)
            return Fail("Password must be at least 8 characters");

        var token = await _userTokenService.ValidateAsync(request.Token, UserTokenType.PasswordReset);

        if (token == null)
            return Fail("Invalid or expired token");

        var user = token.User;
        user.PasswordHash = _passwordHashService.HashPassword(request.NewPassword);

        await _userTokenService.MarkUsedAsync(token);
        await _refreshTokenService.RevokeAllUserTokensAsync(user.Id, ipAddress);

        await _auditLog.LogAsync(
            SecurityEvent.PasswordResetCompleted, true, ipAddress,
            userId: user.Id, username: user.Username);

        return new AuthResponse { Success = true, Message = "Password updated. All sessions have been revoked." };
    }

    public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Fail("Token is required");

        var token = await _userTokenService.ValidateAsync(request.Token, UserTokenType.EmailVerification);

        if (token == null)
            return Fail("Invalid or expired token");

        var user = token.User;

        if (user.IsEmailVerified)
        {
            await _userTokenService.MarkUsedAsync(token);
            return new AuthResponse { Success = true, Message = "Email already verified" };
        }

        user.IsEmailVerified = true;
        await _userTokenService.MarkUsedAsync(token);

        await _auditLog.LogAsync(
            SecurityEvent.EmailVerified, true, ipAddress,
            userId: user.Id, username: user.Username);

        return new AuthResponse { Success = true, Message = "Email verified successfully" };
    }

    private static AuthResponse Fail(string message)
        => new() { Success = false, Message = message };
}
