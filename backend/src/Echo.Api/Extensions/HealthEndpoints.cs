using Microsoft.Extensions.Diagnostics.HealthChecks;
using static Echo.Api.Extensions.Health;

namespace Echo.Api.Extensions;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health").AllowAnonymous();

        // Liveness: "is this process up?". Must not depend on external services; a failure
        // means restart.
        group
            .MapGet(
                "/live",
                async (HealthCheckService health, CancellationToken ct) =>
                    Respond(
                        await health.CheckHealthAsync(
                            reg => reg.Tags.Contains(HealthCheckTags.Liveness),
                            ct
                        )
                    )
            )
            .WithName("HealthLive")
            .WithSummary("Liveness probe: is the process up?");

        // Readiness: "should traffic be routed here?". Runs every check; a failure means take
        // out of rotation, do not restart.
        group
            .MapGet(
                "/ready",
                async (HealthCheckService health, CancellationToken ct) =>
                    Respond(await health.CheckHealthAsync(_ => true, ct))
            )
            .WithName("HealthReady")
            .WithSummary("Readiness probe: are the dependencies reachable?");

        return app;
    }

    private static IResult Respond(HealthReport report)
    {
        var body = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
            }),
        };

        var statusCode =
            report.Status == HealthStatus.Unhealthy
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status200OK;
        return TypedResults.Json(body, statusCode: statusCode);
    }
}
