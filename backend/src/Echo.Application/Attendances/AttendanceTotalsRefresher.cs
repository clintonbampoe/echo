using System.Diagnostics;
using Echo.Application.Caching;
using Echo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Attendances;

/// <summary>
/// Brings the <c>attendance_per_session</c> view up to date every minute, so the attendance
/// and session summaries can add up a few rows per session instead of counting every check-in. A
/// failed refresh is logged and tried again next time; the summaries keep the last totals. Each refresh
/// changes figures no write to a congregation announces, so it drops the cached answers that read the views.
/// </summary>
public sealed class AttendanceTotalsRefresher(
    IServiceScopeFactory scopes,
    ReadCacheEvictions cache,
    TimeProvider time,
    ILogger<AttendanceTotalsRefresher> logger
) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    // Any number, as long as nothing else locks on it. With several API instances, whoever gets it
    // refreshes and the rest skip this round.
    private const long LockKey = 7_301_001;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, time);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await Refresh(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    internal async Task Refresh(CancellationToken ct)
    {
        try
        {
            var started = Stopwatch.GetTimestamp();
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // The lock lasts as long as the transaction, so it cannot be left behind.
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var locked = await db
                .Database.SqlQuery<bool>(
                    $"SELECT pg_try_advisory_xact_lock({LockKey}) AS \"Value\""
                )
                .SingleAsync(ct);
            if (!locked)
                return;

            // CONCURRENTLY keeps the old rows readable while the new ones are built.
            await db.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY attendance_per_session",
                ct
            );
            // The event list reads its registration and attendance counts the same way, and the project list its gift totals.
            await db.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY event_counts",
                ct
            );
            await db.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY project_totals",
                ct
            );
            await transaction.CommitAsync(ct);
            await cache.Drop(ReadCache.Views, ct);

            AttendanceLog.TotalsRefreshed(
                logger,
                (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds
            );
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            AttendanceLog.TotalsRefreshFailed(logger, ex);
        }
    }
}
