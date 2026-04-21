using System.Security.Cryptography;
using System.Text;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IRefreshTokenService
{
    Task<(RefreshToken entity, string rawToken)> CreateAsync(int userId, string ipAddress);
    Task<(RefreshToken entity, string rawToken)> RotateAsync(RefreshToken current, string ipAddress);
    Task<RefreshToken?> FindByRawTokenAsync(string rawToken);
    Task RevokeAsync(RefreshToken token, string ipAddress);
    Task RevokeFamilyAsync(string family, string ipAddress);
    Task RevokeAllUserTokensAsync(int userId, string ipAddress);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly int _expirationDays;

    private const int TokenByteLength = 64;

    public RefreshTokenService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _expirationDays = int.Parse(configuration["RefreshToken:ExpirationDays"] ?? "7");
    }

    public async Task<(RefreshToken entity, string rawToken)> CreateAsync(int userId, string ipAddress)
    {
        var rawToken = GenerateRawToken();

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(rawToken),
            Family = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(_expirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return (entity, rawToken);
    }

    public async Task<(RefreshToken entity, string rawToken)> RotateAsync(RefreshToken current, string ipAddress)
    {
        current.RevokedAt = DateTime.UtcNow;
        current.RevokedByIp = ipAddress;

        var rawToken = GenerateRawToken();

        var next = new RefreshToken
        {
            UserId = current.UserId,
            TokenHash = HashToken(rawToken),
            Family = current.Family,
            ExpiresAt = DateTime.UtcNow.AddDays(_expirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(next);
        await _db.SaveChangesAsync();

        return (next, rawToken);
    }

    public async Task<RefreshToken?> FindByRawTokenAsync(string rawToken)
    {
        var hash = HashToken(rawToken);
        return await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash);
    }

    public async Task RevokeAsync(RefreshToken token, string ipAddress)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _db.SaveChangesAsync();
    }

    public async Task RevokeFamilyAsync(string family, string ipAddress)
    {
        var active = await _db.RefreshTokens
            .Where(rt => rt.Family == family && rt.RevokedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var token in active)
        {
            token.RevokedAt = now;
            token.RevokedByIp = ipAddress;
        }

        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId, string ipAddress)
    {
        var active = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var token in active)
        {
            token.RevokedAt = now;
            token.RevokedByIp = ipAddress;
        }

        await _db.SaveChangesAsync();
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[TokenByteLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}
