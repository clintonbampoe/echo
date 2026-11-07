using System.Diagnostics;
using Echo.Data;
using Microsoft.EntityFrameworkCore;

namespace Echo.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    IConfiguration configuration,
    ILogger<Worker> logger
) : BackgroundService
{
    // The exit code of `--check=true` when migrations are waiting. 0 means the database is current; 1 is a failure.
    public const int PendingExitCode = 10;

    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _sActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var activity = _sActivitySource.StartActivity("worker.migrator", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (configuration.GetValue<bool>("check"))
                await CheckAsync(context, ct);
            else
                await RunMigrationAsync(context, ct);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            // The host stops on a thrown exception but still exits 0. A deploy must see a failed migration as one.
            Environment.ExitCode = 1;
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    // Lists the migrations the database has not had yet, and changes nothing. A deploy runs this first to know
    // whether to take a backup before the real run.
    private async Task CheckAsync(AppDbContext dbContext, CancellationToken ct)
    {
        var pending = (await dbContext.Database.GetPendingMigrationsAsync(ct)).ToList();
        foreach (var name in pending)
            logger.LogInformation("Pending migration: {Migration}", name);

        logger.LogInformation("{Count} pending migration(s)", pending.Count);
        if (pending.Count > 0)
            Environment.ExitCode = PendingExitCode;
    }

    private static async Task RunMigrationAsync(AppDbContext dbContext, CancellationToken ct)
    {
        // The execution strategy handles transient network retries safely.
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // EF Core internally handles transactions per-migration step.
            // Do not wrap this in a separate manual transaction block.
            await dbContext.Database.MigrateAsync(ct);
        });
    }
}
