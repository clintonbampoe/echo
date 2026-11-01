using Echo.Application.Assets;
using Echo.Domain.Assets;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Assets;

[Trait("Category", "Unit")]
public class AssetWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Asset BoughtOn(DateOnly date)
    {
        var asset = TestData.Shared.CreateAsset();
        asset.PurchaseDate = date;
        return asset;
    }

    [Fact]
    public void ResolveWindow_Default_IsThisYear()
    {
        var w = AssetService.ResolveWindow(new AssetFilters(), _windows);

        Assert.Equal(new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true), w);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            BoughtOn(new DateOnly(2025, 12, 31)),
            BoughtOn(new DateOnly(2026, 1, 1)),
            BoughtOn(new DateOnly(2026, 12, 31)),
            BoughtOn(new DateOnly(2027, 1, 1)),
        };
        var filters = new AssetFilters
        {
            From = new DateOnly(2026, 1, 1),
            To = new DateOnly(2026, 12, 31),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }
}
