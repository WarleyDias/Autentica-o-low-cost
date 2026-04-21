namespace AuthSystem.Models;

public static class SecurityEvent
{
    public const string LoginSuccess = "LOGIN_SUCCESS";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string TokenReuseDetected = "TOKEN_REUSE_DETECTED";
    public const string TokenRevoked = "TOKEN_REVOKED";
    public const string JwtValidationFailed = "JWT_VALIDATION_FAILED";
}

public class AuditLog
{
    public long Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
