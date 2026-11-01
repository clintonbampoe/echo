using System.Diagnostics;
using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.Api.Extensions;

/// <summary>
/// Does the slow first-time work before the API takes requests: builds EF Core's model, opens the first
/// database connection and runs one query through the LINQ pipeline. Without it the first person to arrive after a
/// deploy pays for all of that (several hundred milliseconds). Hosted services start before the server listens, so
/// the container does not report healthy, and nginx does not send traffic, until this is done. If the database is
/// down the warm-up gives up and logs; the health check reports that on its own.
/// </summary>
public sealed partial class StartupWarmup(
    IServiceScopeFactory scopes,
    ILogger<StartupWarmup> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _ = db.Model;
            await db
                .Congregations.AsNoTracking()
                .Select(c => c.Id)
                .Take(1)
                .ToListAsync(cancellationToken);

            var elapsedMs = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            Warmed(logger, elapsedMs);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            WarmupFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Start-up warm-up finished in {ElapsedMs} ms"
    )]
    private static partial void Warmed(ILogger logger, long elapsedMs);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Start-up warm-up failed; the first requests will be slower"
    )]
    private static partial void WarmupFailed(ILogger logger, Exception ex);
}
