using System.Data;
using System.Diagnostics;
using Echo.Data;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;
using Echo.Seeder.Phases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Echo.Seeder;

/// <summary>
/// Orchestrates the seed pipeline.
///
/// Phase order matters — each phase depends on state produced by the ones before it:
/// <br/> <br/>
///
/// The seeder stops on the first failure. Phases that fail partway through
/// leave partial data — run with --reset to clean up before retrying.
/// </summary>
public sealed class SeederWorker(
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    IOptions<SeederOptions> options,
    ILogger<SeederWorker> logger
) : BackgroundService
{
    public const string ActivitySourceName = "Echo.Seeder";
    private static readonly ActivitySource _source = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var activity = _source.StartActivity("seeder.run", ActivityKind.Internal);
        var sw = Stopwatch.StartNew();

        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
            var opts = options.Value;

            Log.StartingSeeder(logger, opts.Seed, opts.Reset, opts.BatchSize, opts.ReferenceNow);

            if (opts.Reset)
            {
                logger.LogInformation("Reset requested — truncating all seeded tables.");
                await ResetAsync(db, ct);
            }
            else
            {
                await GuardAgainstExistingDataAsync(db, ct);
            }

            // Open one connection for the whole run. All COPY phases share it.
            // EF (SetupPhase) uses db.Database.GetDbConnection() which returns
            // the same underlying connection, so there's no conflict.
            var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            var ctx = new SeederContext(db, conn, opts, timeProvider, logger);
            var fakers = new SeedFakers(opts.Seed);

            // The pipeline. Add or remove phases here.
            ISeedPhase[] phases =
            [
                new SetupPhase(fakers),
                new PeoplePhase(fakers),
                new OrganizationsPhase(),
                new SessionsPhase(),
                new EventsPhase(),
                new AttendancePhase(),
                new FinancesPhase(),
                new AssetsPhase(fakers),
            ];

            foreach (var phase in phases)
            {
                using var phaseActivity = _source.StartActivity($"seeder.phase.{phase.Name}");
                var phaseSw = Stopwatch.StartNew();

                Log.PhaseStart(logger, phase.Name);
                await phase.RunAsync(ctx, ct);
                Log.PhaseComplete(logger, phase.Name, phaseSw.Elapsed.TotalSeconds);
            }

            Log.SeederComplete(logger, sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            Log.SeederError(logger, ex, sw.Elapsed.TotalSeconds);
            activity?.AddException(ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }

    private static async Task GuardAgainstExistingDataAsync(AppDbContext db, CancellationToken ct)
    {
        var count = await db.Congregations.CountAsync(ct);
        if (count > 0)
            throw new InvalidOperationException(
                $"Database already has {count} congregation(s). Pass Reset=true to clear before seeding."
            );
    }

    private static async Task ResetAsync(AppDbContext db, CancellationToken ct)
    {
        // TRUNCATE CASCADE clears each named table and all tables that
        // reference it via FK, recursively. Order doesn't matter with CASCADE
        // but listing leaf tables first avoids surprise if CASCADE is removed.
        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                "Assets",
                "AssetCategories",
                "ProjectContributions",
                "Projects",
                "ProjectCategories",
                "Tithes",
                "Transactions",
                "TransactionCategories",
                "EventAttendances",
                "EventRegistrations",
                "Events",
                "Attendance",
                "Sessions",
                "SessionLabels",
                "OrganizationMembers",
                "Organizations",
                "Visitors",
                "Members",
                "People",
                "Users",
                "Congregations"
            RESTART IDENTITY CASCADE
            """,
            ct
        );
    }
}

public static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Seeder starting — Seed={Seed} Reset={Reset} BatchSize={Batch} ReferenceNow={Now:o}"
    )]
    public static partial void StartingSeeder(
        this ILogger logger,
        int seed,
        bool reset,
        int batch,
        DateTimeOffset now
    );

    [LoggerMessage(Level = LogLevel.Information, Message = "── Phase: {Phase} ──")]
    public static partial void PhaseStart(this ILogger logger, string phase);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "── Phase: {Phase} completed in {Elapsed:F1}s ──"
    )]
    public static partial void PhaseComplete(this ILogger logger, string phase, double elapsed);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeder completed in {Elapsed:F1}s.")]
    public static partial void SeederComplete(this ILogger logger, double elapsed);

    [LoggerMessage(Level = LogLevel.Error, Message = "Seeder failed after {Elapsed:F1}s.")]
    public static partial void SeederError(this ILogger logger, Exception ex, double elapsed);
}
