using Echo.Application.Tithes;
using Echo.Domain.Tithes;
using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Tithes;

[Trait("Category", "Unit")]
public class TitheWindowTests
{
    private static readonly DateWindows _windows = new(
        new FakeTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))
    );

    private static Tithe On(DateOnly collectionDate)
    {
        var tithe = TitheFactory.NewEntity();
        tithe.CollectionDate = collectionDate;
        return tithe;
    }

    [Fact]
    public void Filter_NoDates_AppliesNoDateRestriction()
    {
        var rows = new[] { On(new DateOnly(2020, 1, 1)), On(new DateOnly(2026, 10, 14)) };

        var res = rows.AsQueryable().Filter(new TitheFilters()).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void Filter_FromAndTo_KeepsOnlyRowsInsideInclusive()
    {
        var rows = new[]
        {
            On(new DateOnly(2026, 10, 10)),
            On(new DateOnly(2026, 10, 11)),
            On(new DateOnly(2026, 10, 14)),
            On(new DateOnly(2026, 10, 15)),
        };
        var filters = new TitheFilters
        {
            From = new DateOnly(2026, 10, 11),
            To = new DateOnly(2026, 10, 14),
        };

        var res = rows.AsQueryable().Filter(filters).ToList();

        Assert.Equal(2, res.Count);
    }

    [Fact]
    public void ResolveWindow_Unscoped_DefaultsToThisWeekToDate()
    {
        var w = TitheService.ResolveWindow(new TitheFilters(), _windows);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 14), true),
            w
        );
    }

    [Fact]
    public void ResolveWindow_MemberIdSet_ReturnsNull()
    {
        var filters = new TitheFilters { MemberId = Constants.DefaultGuid };

        Assert.Null(TitheService.ResolveWindow(filters, _windows));
    }
}
