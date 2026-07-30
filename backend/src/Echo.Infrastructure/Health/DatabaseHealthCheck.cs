using Echo.Domain.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Infrastructure.Health;

public sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    private readonly AppDbContext _appDbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var canConnect = await _appDbContext.Database.CanConnectAsync(ct);
            return canConnect
                ? HealthCheckResult.Healthy("Database Reachable")
                : HealthCheckResult.Unhealthy("Database Unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check threw", ex);
        }
    }
}
