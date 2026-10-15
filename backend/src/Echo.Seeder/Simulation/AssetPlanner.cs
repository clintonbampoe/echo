using Echo.Domain.Assets;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Plans a congregation's assets, bought over its history.
/// <br/><br/>
/// What it owns comes from <see cref="AssetCatalog"/>, in numbers that suit
/// its size. Most things were bought recently (old equipment gets replaced),
/// buildings and land early. Older purchases cost less in cedis, as prices have
/// risen. Value falls with age down to a tenth of cost, and land rises. Old
/// things are more often in storage, being repaired or sold.
/// </summary>
public sealed class AssetPlanner(CongregationSeed seed, Rng rng, DateOnly today, DateTime now)
{
    private static readonly string[] _plates = ["GR", "GT", "GN", "GW", "AS"];

    private static readonly string[] _origins =
    [
        "Donated by a member",
        "Gift from a partner church",
        "Bought from the building fund",
        "Bought from offerings",
    ];

    private readonly int _daysOfHistory = today.DayNumber - seed.Founded.DayNumber;

    public List<Asset> Plan()
    {
        var categories = seed.AssetCategories.ToDictionary(c => c.Name);
        var assets = new List<Asset>();

        foreach (var kind in AssetCatalog.All)
        {
            if (!categories.TryGetValue(kind.Category, out var category))
                continue;

            var count = CountOf(kind);
            for (var i = 1; i <= count; i++)
            {
                var name = count > 1 ? $"{kind.Name} {i}" : kind.Name;
                assets.Add(Build(kind, category, name));
            }
        }

        return assets;
    }

    // The big congregation owns what the catalog says; a small one owns fewer or more depending on its size.
    private int CountOf(AssetKind kind)
    {
        if (seed.IsBig)
            return rng.Next(kind.MinBig, kind.MaxBig + 1);

        var size = Math.Clamp(seed.Members.Count / 250.0, 0.4, 2.0);
        return (int)Math.Round(rng.Next(kind.MinSmall, kind.MaxSmall + 1) * size);
    }

    private Asset Build(AssetKind kind, AssetCategory category, string name)
    {
        var age = AgeInYears(kind);
        var bought = today.AddDays(-(int)(age * 365.25));
        var cost = Money.RoundNicely(kind.Cost * rng.LogNormal(1.0, 0.25) * Math.Pow(0.85, age));
        var status = StatusFor(kind, age);

        return new Asset
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            CategoryId = category.Id,
            Name = name,
            SerialNumber = SerialFor(kind, bought),
            // Older purchases are sometimes not recorded.
            PurchaseDate = age > 5 && rng.Chance(0.1) ? null : bought,
            PurchaseCost = cost,
            CurrentValue = status == AssetStatus.Liquidated ? 0m : ValueNow(kind, cost, age),
            Status = status,
            Description = rng.Chance(0.15) ? rng.Pick(_origins) : null,
            CreatedAt = Stamps.On(bought.AddDays(rng.Next(0, 8)), rng, now),
        };
    }

    // Buildings and land were bought early; everything else mostly recently,
    // because old equipment gets replaced.
    private double AgeInYears(AssetKind kind)
    {
        var oldest = Math.Max(0.1, (_daysOfHistory - 14) / 365.25);

        if (kind.Trend == ValueTrend.Appreciates || kind.LifeYears >= 40)
            return Math.Min(oldest, oldest * rng.Range(0.3, 1.0));

        return Math.Min(oldest, rng.Exponential(kind.LifeYears * 0.55) + 0.05);
    }

    private decimal ValueNow(AssetKind kind, decimal cost, double age)
    {
        var condition = rng.Range(0.9, 1.05);

        if (kind.Trend == ValueTrend.Appreciates)
            return Money.Cents((double)cost * Math.Pow(1.12, age));

        var remaining = Math.Max(0.1, 1.0 - 0.9 * age / kind.LifeYears);
        return Money.Cents(Math.Min((double)cost, (double)cost * remaining * condition));
    }

    private AssetStatus StatusFor(AssetKind kind, double age)
    {
        // Things that get repaired a lot.
        var repairs = kind.Category is "Vehicles" or "Generators" or "Air Conditioning" ? 8.0 : 4.0;

        if (kind.LifeYears > 0 && age > kind.LifeYears)
        {
            return rng.Weighted<AssetStatus>([
                (AssetStatus.Liquidated, 40.0),
                (AssetStatus.InStorage, 20.0),
                (AssetStatus.UnderMaintenance, 10.0),
                (AssetStatus.InUse, 30.0),
            ]);
        }

        return rng.Weighted<AssetStatus>([
            (AssetStatus.Liquidated, 2.0),
            (AssetStatus.InStorage, 10.0),
            (AssetStatus.UnderMaintenance, repairs),
            (AssetStatus.Active, 15.0),
            (AssetStatus.InUse, 69.0 - repairs),
        ]);
    }

    private string? SerialFor(AssetKind kind, DateOnly bought)
    {
        if (kind.Serial is null || !rng.Chance(0.9))
            return null;

        var year = bought.Year % 100;

        return kind.Serial == "PLATE"
            ? $"{rng.Pick(_plates)} {rng.Next(100, 9999)}-{year:00}"
            : $"{kind.Serial}{year:00}-{rng.Next(100000, 999999)}";
    }
}
