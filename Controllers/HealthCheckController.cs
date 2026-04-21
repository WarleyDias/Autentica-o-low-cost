using Microsoft.AspNetCore.Mvc;
using AuthSystem.Services;
using AuthSystem.Models;

namespace AuthSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthCheckResponse> GetHealth()
    {
        var response = _healthCheckService.GetHealthStatus();
        return Ok(response);
    }

    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Liveness()
    {
        return Ok(new { status = "alive" });
    }

    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Readiness()
    {
        var response = _healthCheckService.GetHealthStatus();
        
        if (response.Details.DatabaseHealthy && response.Details.AuthServiceHealthy)
        {
            return Ok(new { status = "ready" });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, 
            new { status = "not ready", details = response.Details });
    }
}
