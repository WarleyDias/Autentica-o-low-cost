using System.Security.Cryptography;
using System.Text;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IUserTokenService
{
    Task<string> CreateAsync(int userId, UserTokenType tokenType);
    Task<UserToken?> ValidateAsync(string rawToken, UserTokenType expectedType);
    Task MarkUsedAsync(UserToken token);
}

public class UserTokenService : IUserTokenService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<UserTokenType, TimeSpan> Expirations = new()
    {
        [UserTokenType.EmailVerification] = TimeSpan.FromHours(24),
        [UserTokenType.PasswordReset] = TimeSpan.FromHours(1)
    };

    public UserTokenService(AppDbContext db) => _db = db;

    public async Task<string> CreateAsync(int userId, UserTokenType tokenType)
    {
        var existing = await _db.UserTokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType && t.UsedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var t in existing)
            t.UsedAt = now;

        var rawToken = GenerateRawToken();

        _db.UserTokens.Add(new UserToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            TokenType = tokenType,
            ExpiresAt = now.Add(Expirations[tokenType]),
            CreatedAt = now
        });

        await _db.SaveChangesAsync();

        return rawToken;
    }

    public async Task<UserToken?> ValidateAsync(string rawToken, UserTokenType expectedType)
    {
        var hash = HashToken(rawToken);
        var token = await _db.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.TokenType == expectedType);

        return token?.IsValid == true ? token : null;
    }

    public async Task MarkUsedAsync(UserToken token)
    {
        token.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}
