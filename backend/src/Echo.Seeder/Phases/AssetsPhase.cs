using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Writes each congregation's assets.
/// <br/><br/>
/// The rules live in <see cref="AssetPlanner"/>: what a congregation owns
/// depends on its size, things were bought over its history, and value and
/// status follow age. Asset names are unique per congregation (unique index on
/// CongregationId + Name), which the numbering in each name guarantees.
/// <br/><br/>
/// Output written to state: AssetIds.
/// </summary>
public sealed class AssetsPhase : ISeedPhase
{
    public string Name => "Assets";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
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

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var assets = new AssetPlanner(seed, rng, ctx.Today, ctx.Now).Plan();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                assets,
                ctx.Options.BatchSize,
                FinancesCopy.WriteAssetsAsync,
                null,
                ct
            );
            seed.AssetIds.AddRange(assets.Select(a => a.Id));

            ctx.Logger.AssetsDone(seed.Congregation.Name, assets.Count);
        }
    }
}
