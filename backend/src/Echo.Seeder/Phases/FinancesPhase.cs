using Echo.Domain.Projects;
using Echo.Domain.Tithes;
using Echo.Domain.Transactions;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates tithes, transactions, projects, and project contributions.
///
/// All amounts use decimal with 2dp to match the Numeric columns.
/// Payment method weights are skewed toward Cash and MobileMoney to model
/// a typical congregation in a mobile-money-heavy context.
///
/// ProjectContribution has no MemberId — contributions are anonymous
/// (as confirmed by the entity definition).
///
/// Output written to state: ProjectIds on each CongregationSeed.
/// </summary>
public sealed class FinancesPhase : ISeedPhase
{
    public string Name => "Finances";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.MemberIds.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members, skipping finances.",
                    seed.Congregation.Name
                );
                continue;
            }

            ctx.Logger.LogInformation("Finances for {Congregation}.", seed.Congregation.Name);

            var memberList = seed.MemberIds.ToList();

            await WriteTithesAsync(ctx, rng, seed, memberList, ct);
            await WriteTransactionsAsync(ctx, rng, seed, ct);
            await WriteProjectsAsync(ctx, rng, seed, memberList, ct);
        }
    }

    // ── Tithes ────────────────────────────────────────────────────────────

    private static async Task WriteTithesAsync(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed,
        IReadOnlyList<Guid> memberList,
        CancellationToken ct
    )
    {
        var count = seed.IsBig ? 100_000 : 30_000;

        var source = Enumerable
            .Range(0, count)
            .Select(_ =>
            {
                var collectedAt = ctx.Now.AddDays(-rng.Next(0, 730));
                var period = collectedAt.AddDays(-rng.Next(0, 30));

                return new Tithe
                {
                    Id = Guid.NewGuid(),
                    CongregationId = seed.Id,
                    MemberId = rng.Pick(memberList),
                    Amount = (decimal)Math.Round(rng.NextDouble() * 990 + 10, 2),
                    ForYear = period.Year,
                    ForMonth = (MonthOfYear)period.Month,
                    PaymentMethod = rng.Weighted(_paymentWeights),
                    CollectionDate = DateOnly.FromDateTime(collectedAt),
                    Description = null,
                    CreatedAt = ctx.Now,
                };
            });

        await CopyBatch.WriteAsync(
            ctx.Connection,
            source,
            ctx.Options.BatchSize,
            FinancesCopy.WriteTithesAsync,
            total => ctx.Logger.LogInformation("  tithes: {N}", total),
            ct
        );
    }

    // ── Transactions ──────────────────────────────────────────────────────

    private static async Task WriteTransactionsAsync(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed,
        CancellationToken ct
    )
    {
        var count = seed.IsBig ? 20_000 : 5000;
        var categories = seed.TransactionCategories;

        if (categories.Count == 0)
            return;

        var source = Enumerable
            .Range(0, count)
            .Select(_ =>
            {
                var cat = rng.Pick(categories);
                return new Transaction
                {
                    Id = Guid.NewGuid(),
                    CongregationId = seed.Id,
                    CategoryId = cat.Id,
                    TransactionType = cat.CategoryType,
                    TransactionDate = DateOnly.FromDateTime(ctx.Now.AddDays(-rng.Next(0, 730))),
                    Amount = (decimal)Math.Round(rng.NextDouble() * 4900 + 100, 2),
                    Description = null,
                    CreatedAt = ctx.Now,
                };
            });

        await CopyBatch.WriteAsync(
            ctx.Connection,
            source,
            ctx.Options.BatchSize,
            FinancesCopy.WriteTransactionsAsync,
            total => ctx.Logger.LogInformation("  transactions: {N}", total),
            ct
        );
    }

    // ── Projects + contributions ──────────────────────────────────────────

    private static async Task WriteProjectsAsync(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed,
        IReadOnlyList<Guid> memberList,
        CancellationToken ct
    )
    {
        var projCount = seed.IsBig ? rng.Next(30, 500) : rng.Next(2, 50);
        var categories = seed.ProjectCategories;

        if (categories.Count == 0)
            return;

        var projects = Enumerable
            .Range(0, projCount)
            .Select(_ =>
            {
                var startDate = DateOnly.FromDateTime(ctx.Now.AddDays(-rng.Next(60, 900)));
                return new Project
                {
                    Id = Guid.NewGuid(),
                    CongregationId = seed.Id,
                    CategoryId = rng.Pick(categories).Id,
                    ManagerId = rng.Pick(memberList),
                    Name = rng.Pick(_projectStems),
                    TargetAmount = (decimal)Math.Round(rng.NextDouble() * 90_000 + 10_000, 2),
                    Status = rng.Weighted(_projectStatusWeights),
                    StartDate = startDate,
                    EndDate = rng.Chance(0.8) ? startDate.AddDays(rng.Next(60, 720)) : null,
                    Description = null,
                    CreatedAt = ctx.Now,
                };
            })
            .ToList();

        await CopyBatch.WriteAsync(
            ctx.Connection,
            projects,
            ctx.Options.BatchSize,
            FinancesCopy.WriteProjectsAsync,
            null,
            ct
        );

        foreach (var p in projects)
            seed.ProjectIds.Add(p.Id);

        // ── Project contributions ─────────────────────────────────────────

        var contribCount = seed.IsBig ? 20_000 : 5000;

        var contributions = Enumerable
            .Range(0, contribCount)
            .Select(_ => new ProjectContribution
            {
                Id = Guid.NewGuid(),
                CongregationId = seed.Id,
                ProjectId = rng.Pick(seed.ProjectIds),
                Amount = (decimal)Math.Round(rng.NextDouble() * 4900 + 50, 2),
                DateContributed = DateOnly.FromDateTime(ctx.Now.AddDays(-rng.Next(0, 600))),
                PaymentMethod = rng.Weighted(_paymentWeights),
                Description = null,
                CreatedAt = ctx.Now,
            });

        await CopyBatch.WriteAsync(
            ctx.Connection,
            contributions,
            ctx.Options.BatchSize,
            FinancesCopy.WriteProjectContributionsAsync,
            total => ctx.Logger.LogInformation("  contributions: {N}", total),
            ct
        );

        ctx.Logger.LogInformation(
            "{Congregation}: {P} projects, {C} contributions.",
            seed.Congregation.Name,
            projects.Count,
            contribCount
        );
    }

    // ── Reference data ────────────────────────────────────────────────────

    private static readonly IReadOnlyList<(PaymentMethod Value, double Weight)> _paymentWeights =
    [
        (PaymentMethod.Cash, 40.0),
        (PaymentMethod.MobileMoney, 35.0),
        (PaymentMethod.BankTransfer, 15.0),
        (PaymentMethod.Cheque, 5.0),
        (PaymentMethod.CreditCard, 5.0),
    ];

    private static readonly IReadOnlyList<(
        ProjectStatus Value,
        double Weight
    )> _projectStatusWeights =
    [
        (ProjectStatus.Planning, 15.0),
        (ProjectStatus.OnTrack, 40.0),
        (ProjectStatus.AtRisk, 15.0),
        (ProjectStatus.Complete, 25.0),
        (ProjectStatus.Missed, 5.0),
    ];

    private static readonly IReadOnlyList<string> _projectStems =
    [
        "Sanctuary Expansion",
        "Youth Center Build",
        "New Auditorium",
        "Bus Purchase",
        "Borehole Project",
        "Solar Installation",
        "Missions Fund",
        "Widows Support",
        "Scholarship Fund",
        "Community Outreach",
        "Land Acquisition",
        "Building Renovation",
    ];
}
