using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string eventType,
        bool success,
        string ipAddress,
        int? userId = null,
        string? username = null,
        string? details = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IDbContextFactory<AppDbContext> dbFactory, ILogger<AuditLogService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task LogAsync(
        string eventType,
        bool success,
        string ipAddress,
        int? userId = null,
        string? username = null,
        string? details = null)
    {
        var level = eventType == SecurityEvent.TokenReuseDetected
            ? LogLevel.Critical
            : !success
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(
            level,
            "[AUDIT] Event={EventType} Success={Success} UserId={UserId} Username={Username} IP={IpAddress} Details={Details}",
            eventType, success, userId, username, ipAddress, details);

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            db.AuditLogs.Add(new AuditLog
            {
                EventType = eventType,
                Success = success,
                IpAddress = ipAddress,
                UserId = userId,
                Username = username,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AUDIT] Failed to persist audit log entry {EventType}", eventType);
        }
    }
}
