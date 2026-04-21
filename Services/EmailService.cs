namespace AuthSystem.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string username, string rawToken);
    Task SendEmailVerificationAsync(string toEmail, string username, string rawToken);
}

public class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;

    public LogEmailService(ILogger<LogEmailService> logger) => _logger = logger;

    public Task SendPasswordResetAsync(string toEmail, string username, string rawToken)
    {
        _logger.LogInformation(
            "[EMAIL] To={Email} Subject='Password Reset' Username={Username} Token={Token} ExpiresIn=1h",
            toEmail, username, rawToken);

        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string username, string rawToken)
    {
        _logger.LogInformation(
            "[EMAIL] To={Email} Subject='Verify your email' Username={Username} Token={Token} ExpiresIn=24h",
            toEmail, username, rawToken);

        return Task.CompletedTask;
    }
}
