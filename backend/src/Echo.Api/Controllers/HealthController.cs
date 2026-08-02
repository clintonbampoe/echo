using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using static Echo.Api.Extensions.HealthCheckExtensions;

namespace Echo.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    private readonly HealthCheckService _healthCheckService = healthCheckService;

    /// <summary>
    /// Liveness probe - answers the question "Is this process up?".
    /// SHOULD NOT depend on external services. Failure here means restart.
    /// </summary>
    [HttpGet("live")]
    public async Task<IActionResult> Live(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(predicate: reg => reg.Tags.Contains(HealthCheckTags.Liveness), ct);

        return BuildResponse(report);
    }

    /// <summary>
    /// Readiness probe - answers "Should traffic be routed here?".
    /// Runs every registered check; failure means take out of rotation, DO NOT restart
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Readiness(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(predicate: _ => true, ct);

        return BuildResponse(report);
    }

    private IActionResult BuildResponse(HealthReport report)
    {
        var body = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.Milliseconds,
            checks = report.Entries.Select(kvp => new
            {
                name = kvp.Key,
                status = kvp.Value.Status.ToString(),
                description = kvp.Value.Description,
                durationMs = kvp.Value.Duration.TotalMilliseconds,
                exception = kvp.Value.Exception?.Message
            })
        };

        return report.Status switch
        {
            HealthStatus.Unhealthy => StatusCode(StatusCodes.Status503ServiceUnavailable, body),
            HealthStatus.Degraded => StatusCode(StatusCodes.Status200OK, body),
            _ => Ok(body)
        };
    }
}
