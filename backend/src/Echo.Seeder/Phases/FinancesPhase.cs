using Echo.Domain.Projects;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Writes tithes, projects with their contributions, and the transactions.
/// <br/><br/>
/// The rules live in the simulation classes. <see cref="TitheSimulator"/> turns
/// each member's giving profile into at most one tithe per month.
/// <see cref="ProjectPlanner"/> plans projects whose story fits their status
/// and sizes them from the tithe income. <see cref="LedgerBuilder"/> then
/// writes the transactions so that income matches the tithes, offerings and
/// contributions, and expenses follow income.
/// <br/><br/>
/// Everything covers the same <see cref="FinanceWindow"/> (TitheMonths back
/// from ReferenceNow, or since the congregation began).
/// <br/><br/>
/// Output written to state: ProjectIds.
/// </summary>
public sealed class FinancesPhase : ISeedPhase
{
    public string Name => "Finances";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        foreach (var seed in ctx.State)
        {
            if (seed.Members.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members, skipping finances.",
                    seed.Congregation.Name
                );
                continue;
            }

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var window = FinanceWindow.For(ctx.Today, seed.Founded, ctx.Options.TitheMonths);

            // ── Tithes ──
            var tithes = new TitheSimulator(seed, rng.Fork("tithes"), window, ctx.Now);
            await CopyBatch.WriteAsync(
                ctx.Connection,
                tithes.Generate(),
                ctx.Options.BatchSize,
                FinancesCopy.WriteTithesAsync,
                n =>
                {
                    if (n % (ctx.Options.BatchSize * 50) == 0)
                        ctx.Logger.LogInformation(
                            "  {Congregation} tithes: {N}",
                            seed.Congregation.Name,
                            n
                        );
                },
                ct
            );

            // ── Projects and contributions ──
            var planner = new ProjectPlanner(
                seed,
                rng.Fork("projects"),
                window,
                ctx.Now,
                TypicalMonthlyTithes(tithes.TotalsByDate, window)
            );
            var projects = planner.Build();
            var contributions = projects.SelectMany(p => p.Contributions).ToList();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                projects.Select(p => p.Project),
                ctx.Options.BatchSize,
                FinancesCopy.WriteProjectsAsync,
                null,
                ct
            );
            seed.ProjectIds.AddRange(projects.Select(p => p.Project.Id));

            await CopyBatch.WriteAsync(
                ctx.Connection,
                contributions,
                ctx.Options.BatchSize,
                FinancesCopy.WriteProjectContributionsAsync,
                null,
                ct
            );

            // ── Transactions ──
            var ledger = new LedgerBuilder(
                seed,
                rng.Fork("ledger"),
                window,
                ctx.Now,
                tithes.TotalsByDate,
                projects
            );
            var transactions = ledger.Build();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                transactions,
                ctx.Options.BatchSize,
                FinancesCopy.WriteTransactionsAsync,
                null,
                ct
            );

            ctx.Logger.LogInformation(
                "{Congregation}: {Tithes:N0} tithes from {From}, {Projects} projects, "
                    + "{Contributions:N0} contributions, {Transactions:N0} transactions.",
                seed.Congregation.Name,
                tithes.Count,
                window.Start,
                projects.Count,
                contributions.Count,
                transactions.Count
            );
        }
    }

    // What the congregation received in tithes in a typical month of the last year.
    // Used to size project targets. Falls back to a small figure for a young congregation.
    private static decimal TypicalMonthlyTithes(
        Dictionary<DateOnly, decimal> totalsByDate,
        FinanceWindow window
    )
    {
        var since = window.Today.AddDays(-365);
        var lastYear = totalsByDate.Where(t => t.Key >= since).Sum(t => t.Value);
        var months = Math.Clamp((window.Today.DayNumber - window.Start.DayNumber) / 30, 1, 12);

        return Math.Max(2_000m, lastYear / months);
    }
}
