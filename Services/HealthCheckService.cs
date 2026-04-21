using AuthSystem.Models;

namespace AuthSystem.Services;

public interface IHealthCheckService
{
    HealthCheckResponse GetHealthStatus();
}

public class HealthCheckService : IHealthCheckService
{
    private readonly DateTime _startTime;
    private readonly IConfiguration _configuration;

    public HealthCheckService(IConfiguration configuration)
    {
        _startTime = DateTime.UtcNow;
        _configuration = configuration;
    }

    public HealthCheckResponse GetHealthStatus()
    {
        var uptime = DateTime.UtcNow - _startTime;

        return new HealthCheckResponse
        {
            Status = "Ok",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Details = new HealthCheckDetails
            {
                DatabaseHealthy = true,  // Em produção, verificar conexão real
                AuthServiceHealthy = true,
                UptimeMilliseconds = (long)uptime.TotalMilliseconds,
                ActiveUsers = 0,  // Em produção, contar usuários ativos
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
            }
        };
    }
}
