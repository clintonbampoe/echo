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
/// Runs the seed pipeline, once, then stops the host.
///
/// Phase order matters: each phase reads what the earlier ones left in
/// <c>ctx.State</c>. The seeder stops on the first failure. A phase that fails
/// partway leaves partial data, so run again with Reset=true.
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

            logger.Starting(opts.Seed, opts.Reset, opts.BatchSize, opts.ReferenceNow);

            if (opts.Reset)
            {
                logger.Resetting();
                await SeedDatabase.ResetAsync(db, ct);
            }
            else
            {
                await SeedDatabase.EnsureEmptyAsync(db, ct);
            }

            // One connection for the whole run. The COPY phases share it, and
            // EF (SetupPhase) uses the same underlying connection.
            var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            var ctx = new SeederContext(db, conn, opts, timeProvider, logger);
            var fakers = new SeedFakers(opts.Seed);

            // The pipeline. Add or remove phases here.
            ISeedPhase[] phases =
            [
                new SetupPhase(fakers),
                new PeoplePhase(),
                new OrganizationsPhase(),
                new SessionsPhase(),
                new EventsPhase(),
                new AttendancePhase(),
                new FinancesPhase(),
                new AssetsPhase(),
                new AnalyzePhase(),
            ];

            foreach (var phase in phases)
            {
                using var phaseActivity = _source.StartActivity($"seeder.phase.{phase.Name}");
                var phaseSw = Stopwatch.StartNew();

                logger.PhaseStarted(phase.Name);
                await phase.RunAsync(ctx, ct);
                logger.PhaseDone(phase.Name, phaseSw.Elapsed.TotalSeconds);
            }

            logger.Complete(sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.Failed(ex, sw.Elapsed.TotalSeconds);
            activity?.AddException(ex);
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
