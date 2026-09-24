using Echo.Data.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Api.Extensions;

public static class Health
{
    /// <summary>
    /// Registers the health check services. Individual <see cref="IHealthCheck"/>
    /// implementations are owned by their respective layers.
    /// This method wires the aggregate pipeline and shared options.
    /// </summary>
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<DbHealthCheck>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthCheckTags.Readiness },
                timeout: TimeSpan.FromSeconds(2)
            );

        return services;
    }

    public static class HealthCheckTags
    {
        public const string Liveness = "live";
        public const string Readiness = "ready";
    }
}
