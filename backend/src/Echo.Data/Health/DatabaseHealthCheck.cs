using Echo.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Data.Health;

/// <summary>
/// Readiness check for the database. Answers "can this instance serve traffic?",
/// which requires both a reachable database AND a fully migrated schema.
/// A half-migrated database is reachable but cannot serve requests, so it reports Unhealthy.
/// </summary>
public sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    /// <summary>
    /// A freshly created database reports every migration as pending, so the description
    /// is capped to keep probe responses and log lines bounded.
    /// </summary>
    private const int MaxReportedMigrations = 5;

    private readonly AppDbContext _appDbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var canConnect = await _appDbContext.Database.CanConnectAsync(ct);
            if (!canConnect)
                return HealthCheckResult.Unhealthy("Database Unreachable");

            // Compares the migrations compiled into this build against __EFMigrationsHistory.
            // Single SELECT, no schema scanning.
            var pendingMigrations = (await _appDbContext.Database.GetPendingMigrationsAsync(ct)).ToList();
            if (pendingMigrations.Count > 0)
            {
                return HealthCheckResult.Unhealthy(
                    $"Database has {pendingMigrations.Count} pending migration(s): {Describe(pendingMigrations)}");
            }

            return HealthCheckResult.Healthy("Database reachable, migrations applied");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check threw", ex);
        }
    }

    private static string Describe(IReadOnlyList<string> pendingMigrations)
    {
        var listed = string.Join(", ", pendingMigrations.Take(MaxReportedMigrations));

        return pendingMigrations.Count > MaxReportedMigrations
            ? $"{listed} (+{pendingMigrations.Count - MaxReportedMigrations} more)"
            : listed;
    }
}
