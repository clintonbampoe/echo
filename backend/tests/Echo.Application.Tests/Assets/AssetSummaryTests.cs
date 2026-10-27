using Echo.Application.Assets;
using Echo.Domain.Assets;
using Echo.Shared.Utilities;

namespace Echo.Application.Tests.Assets;

[Trait("Category", "Unit")]
public class AssetSummaryTests
{
    private static readonly DateOnly Today = new(2026, 10, 14);

    private static Asset Make(
        decimal cost,
        decimal value,
        DateOnly? purchased,
        int categoryId = 1,
        string category = "Equipment",
        DateOnly? liquidatedOn = null
    )
    {
        var asset = TestData.Shared.CreateAsset();
        asset.Id = Guid.NewGuid();
        asset.PurchaseCost = cost;
        asset.CurrentValue = value;
        asset.PurchaseDate = purchased;
        asset.CategoryId = categoryId;
        asset.Category.Name = category;
        asset.LiquidatedOn = liquidatedOn;
        return asset;
    }

    [Fact]
    public void LiquidatedOn_WhenNewlyLiquidated_IsToday()
    {
        Assert.Equal(Today, AssetService.LiquidatedOn(AssetStatus.Liquidated, null, Today));
    }

    [Fact]
    public void LiquidatedOn_WhenAlreadyLiquidated_KeepsTheEarlierDate()
    {
        var earlier = new DateOnly(2026, 1, 5);

        Assert.Equal(earlier, AssetService.LiquidatedOn(AssetStatus.Liquidated, earlier, Today));
    }

    [Theory]
    [InlineData(AssetStatus.Active)]
    [InlineData(AssetStatus.InUse)]
    [InlineData(AssetStatus.InStorage)]
    [InlineData(AssetStatus.UnderMaintenance)]
    public void LiquidatedOn_WhenNotLiquidated_IsNone(AssetStatus status)
    {
        Assert.Null(AssetService.LiquidatedOn(status, new DateOnly(2026, 1, 5), Today));
    }

    [Fact]
    public void SummarizeAssets_TotalsTheValuesAndAveragesTheAgeOfThoseWithAPurchaseDate()
    {
        var congregation = Guid.NewGuid();
        var rows = new[]
        {
            Make(100m, 50m, new DateOnly(2024, 10, 14)),
            Make(200m, 150m, new DateOnly(2023, 10, 14)),
            Make(300m, 250m, null),
        };
        foreach (var row in rows)
            row.CongregationId = congregation;

        var res = rows.AsQueryable().SummarizeAssets(Today).Single();

        Assert.Equal((3, 450m, 600m), (res.Count, res.CurrentValue, res.PurchaseCost));
        Assert.Equal(2.5, res.AverageAgeYears, precision: 3);
    }

    [Fact]
    public void SummarizeAssets_WithNoPurchaseDates_HasAnAverageAgeOfZero()
    {
        var asset = Make(100m, 50m, null);

        var res = new[] { asset }.AsQueryable().SummarizeAssets(Today).Single();

        Assert.Equal(0d, res.AverageAgeYears);
    }

    [Fact]
    public void TotalsByCategory_SumsEachCategoryAndSortsByCurrentValueDescending()
    {
        var rows = new[]
        {
            Make(100m, 40m, null, categoryId: 1, category: "Equipment"),
            Make(300m, 60m, null, categoryId: 1, category: "Equipment"),
            Make(900m, 700m, null, categoryId: 2, category: "Vehicles"),
        };
        var categories = rows
            .Select(r => new AssetCategory { Id = r.CategoryId, Name = r.Category.Name })
            .DistinctBy(c => c.Id)
            .AsQueryable();

        var res = rows.AsQueryable().TotalsByCategory(categories).ToList();

        Assert.Equal(["Vehicles", "Equipment"], res.Select(c => c.CategoryName));
        var equipment = res.Single(c => c.CategoryName == "Equipment");
        Assert.Equal((2, 400m, 100m, 300m), (equipment.Count, equipment.TotalPurchaseCost, equipment.TotalCurrentValue, equipment.TotalDepreciation));
    }

    [Fact]
    public void AcquiredByMonth_GroupsByPurchaseMonthAndSkipsAssetsWithNoPurchaseDate()
    {
        var rows = new[]
        {
            Make(100m, 0m, new DateOnly(2026, 3, 2)),
            Make(250m, 0m, new DateOnly(2026, 3, 28)),
            Make(70m, 0m, new DateOnly(2026, 5, 1)),
            Make(999m, 0m, null),
        };

        var res = rows.AsQueryable().AcquiredByMonth().OrderBy(r => r.Month).ToList();

        Assert.Equal([new AssetMonthRow(2026, 3, 2, 350m), new AssetMonthRow(2026, 5, 1, 70m)], res);
    }

    [Fact]
    public void LostByMonth_GroupsByLiquidationMonthAndSkipsAssetsStillOwned()
    {
        var rows = new[]
        {
            Make(100m, 0m, null, liquidatedOn: new DateOnly(2026, 4, 9)),
            Make(40m, 0m, null, liquidatedOn: new DateOnly(2026, 4, 30)),
            Make(500m, 0m, null),
        };

        var res = rows.AsQueryable().LostByMonth().ToList();

        Assert.Equal([new AssetMonthRow(2026, 4, 2, 140m)], res);
    }

    [Fact]
    public void Monthly_ReturnsEveryMonthWithAcquiredAndLostSideBySide()
    {
        var months = MonthRange.Between(new DateOnly(2026, 3, 1), new DateOnly(2026, 5, 31));
        var acquired = new[] { new AssetMonthRow(2026, 3, 2, 350m), new AssetMonthRow(2026, 5, 1, 70m) };
        var lost = new[] { new AssetMonthRow(2026, 5, 3, 90m) };

        var res = AssetService.Monthly(months, acquired, lost);

        Assert.Equal(
            [
                new AssetMonthlyDto { Year = 2026, Month = 3, AcquiredCount = 2, AcquiredCost = 350m },
                new AssetMonthlyDto { Year = 2026, Month = 4 },
                new AssetMonthlyDto { Year = 2026, Month = 5, AcquiredCount = 1, AcquiredCost = 70m, LostCount = 3, LostCost = 90m },
            ],
            res
        );
    }
}
