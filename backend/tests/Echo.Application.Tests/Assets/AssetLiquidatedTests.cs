using Echo.Application.Assets;
using Echo.Domain.Assets;

namespace Echo.Application.Tests.Assets;

[Trait("Category", "Unit")]
public class AssetLiquidatedTests
{
    private static Asset WithStatus(AssetStatus status)
    {
        var asset = AssetFactory.NewEntity();
        asset.Id = Guid.NewGuid();
        asset.Status = status;
        return asset;
    }

    private static readonly Asset[] Rows =
    [
        WithStatus(AssetStatus.Active),
        WithStatus(AssetStatus.InUse),
        WithStatus(AssetStatus.Liquidated),
    ];

    [Fact]
    public void Filter_ShowsLiquidatedAssets_WhenTheFlagIsNotSet()
    {
        var res = Rows.AsQueryable().Filter(new AssetFilters()).ToList();

        Assert.Equal(3, res.Count);
    }

    [Fact]
    public void Filter_ExcludesLiquidatedAssets_WhenIncludeLiquidatedIsFalse()
    {
        var res = Rows.AsQueryable().Filter(new AssetFilters { IncludeLiquidated = false }).ToList();

        Assert.DoesNotContain(res, a => a.Status == AssetStatus.Liquidated);
        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Filter_ShowsLiquidatedAssets_WhenStatusIsLiquidatedEvenIfIncludeLiquidatedIsFalse()
    {
        var filters = new AssetFilters { Status = AssetStatus.Liquidated, IncludeLiquidated = false };

        var res = Rows.AsQueryable().Filter(filters).ToList();

        Assert.Single(res);
        Assert.Equal(AssetStatus.Liquidated, res[0].Status);
    }

    [Fact]
    public void SummaryFilters_ExcludeLiquidatedByDefault()
    {
        Assert.False(AssetService.SummaryFilters(new AssetFilters()).IncludeLiquidated);
    }

    [Fact]
    public void SummaryFilters_KeepAnExplicitIncludeLiquidated()
    {
        var filters = AssetService.SummaryFilters(new AssetFilters { IncludeLiquidated = true });

        Assert.True(filters.IncludeLiquidated);
    }
}
