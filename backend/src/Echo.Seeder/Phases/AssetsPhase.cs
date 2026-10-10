using Echo.Domain.Assets;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates assets for each congregation.
///
/// Assets have a unique index on (CongregationId, Name). We append an index
/// to each name to guarantee uniqueness.
///
/// PurchaseCost >= CurrentValue (assets depreciate). CurrentValue is a random
/// fraction of the purchase cost.
///
/// Output written to state: AssetIds on each CongregationSeed.
/// </summary>
public sealed class AssetsPhase(SeedFakers fakers) : ISeedPhase
{
    public string Name => "Assets";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.AssetCategories.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no asset categories, skipping assets.",
                    seed.Congregation.Name
                );
                continue;
            }

            var count = seed.IsBig ? rng.Next(100, 1000) : rng.Next(20, 200);

            var source = Enumerable
                .Range(0, count)
                .Select(i =>
                {
                    var category = rng.Pick(seed.AssetCategories);
                    var purchaseCost = Math.Round(rng.NextDouble() * 49_000 + 1_000, 2);
                    var depreciationFactor = rng.NextDouble() * 0.5 + 0.5; // 50–100% of cost

                    return new Asset
                    {
                        Id = Guid.NewGuid(),
                        CongregationId = seed.Id,
                        CategoryId = category.Id,
                        // Index appended to ensure uniqueness within congregation
                        Name = $"{category.Name} Asset {i + 1}",
                        SerialNumber = rng.Chance(0.6) ? fakers.Raw.Commerce.Ean13() : null,
                        PurchaseDate = rng.Chance(0.8) ? rng.RecentDate(ctx.Now, 1825) : null,
                        PurchaseCost = (decimal)purchaseCost,
                        CurrentValue = (decimal)Math.Round(purchaseCost * depreciationFactor, 2),
                        Status = rng.Weighted(AssetStatusWeights),
                        Description = null,
                        CreatedAt = ctx.Now,
                    };
                });

            await CopyBatch.WriteAsync(
                ctx.Connection,
                source,
                ctx.Options.BatchSize,
                FinancesCopy.WriteAssetsAsync,
                null,
                ct
            );

            ctx.Logger.AssetsDone(seed.Congregation.Name, count);
        }
    }

    private static readonly IReadOnlyList<(AssetStatus Value, double Weight)> AssetStatusWeights =
    [
        (AssetStatus.Active, 10.0),
        (AssetStatus.InUse, 50.0),
        (AssetStatus.InStorage, 20.0),
        (AssetStatus.UnderMaintenance, 15.0),
        (AssetStatus.Liquidated, 5.0),
    ];
}
