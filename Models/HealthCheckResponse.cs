namespace AuthSystem.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = "Ok";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = "1.0.0";
    public HealthCheckDetails Details { get; set; } = new();
}

public class HealthCheckDetails
{
    public bool DatabaseHealthy { get; set; } = true;
    public bool AuthServiceHealthy { get; set; } = true;
    public long UptimeMilliseconds { get; set; }
    public int ActiveUsers { get; set; }
    public string Environment { get; set; } = "Development";
}
