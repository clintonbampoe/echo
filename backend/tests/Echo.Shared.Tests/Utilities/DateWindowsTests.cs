using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Shared.Tests.Utilities;

[Trait("Category", "Unit")]
public class DateWindowsTests
{
    private static DateWindows At(string utc) =>
        new(new FakeTimeProvider(DateTimeOffset.Parse(utc)));

    private static readonly DateWindows Wednesday = At("2026-10-14T10:00:00Z");

    [Fact]
    public void ThisWeekToDate_NoDates_ReturnsSundayToToday()
    {
        var w = Wednesday.ThisWeekToDate(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 14), true),
            w
        );
    }

    [Fact]
    public void ThisWeek_NoDates_ReturnsSundayToSaturday()
    {
        var w = Wednesday.ThisWeek(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 11), new DateOnly(2026, 10, 17), true),
            w
        );
    }

    [Fact]
    public void ThisMonthToDate_NoDates_ReturnsFirstOfMonthToToday()
    {
        var w = Wednesday.ThisMonthToDate(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 14), true),
            w
        );
    }

    [Fact]
    public void ThisYear_NoDates_ReturnsJanFirstToDecThirtyFirst()
    {
        var w = Wednesday.ThisYear(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), true),
            w
        );
    }

    [Fact]
    public void ThisWeekToDate_OnlyFrom_FillsToWithDefault()
    {
        var w = Wednesday.ThisWeekToDate(new DateOnly(2026, 10, 1), null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 14), false),
            w
        );
    }

    [Fact]
    public void ThisYear_OnlyTo_FillsFromWithDefault()
    {
        var w = Wednesday.ThisYear(null, new DateOnly(2026, 3, 1), hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 1), false),
            w
        );
    }

    [Fact]
    public void AnyMethod_HasParentAndNoDates_ReturnsNull()
    {
        Assert.Null(Wednesday.ThisWeekToDate(null, null, hasParent: true));
        Assert.Null(Wednesday.ThisWeek(null, null, hasParent: true));
        Assert.Null(Wednesday.ThisMonthToDate(null, null, hasParent: true));
        Assert.Null(Wednesday.ThisYear(null, null, hasParent: true));
    }

    [Fact]
    public void AnyMethod_HasParentAndExplicitDates_ReturnsThoseDatesAsNotDefault()
    {
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);

        var w = Wednesday.ThisWeekToDate(from, to, hasParent: true);

        Assert.Equal(new DateWindow(from, to, false), w);
    }

    [Fact]
    public void ThisWeekToDate_YearBoundary_StartsInPreviousYear()
    {
        var newYearsDay = At("2026-01-01T09:00:00Z");

        var w = newYearsDay.ThisWeekToDate(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2025, 12, 28), new DateOnly(2026, 1, 1), true),
            w
        );
    }

    [Fact]
    public void ThisWeekToDate_JustAfterUtcMidnight_MovesToNextDay()
    {
        var before = At("2026-10-14T23:59:59Z").ThisWeekToDate(null, null, hasParent: false);
        var after = At("2026-10-15T00:00:00Z").ThisWeekToDate(null, null, hasParent: false);

        Assert.Equal(new DateOnly(2026, 10, 14), before!.Value.To);
        Assert.Equal(new DateOnly(2026, 10, 15), after!.Value.To);
    }

    [Fact]
    public void LastTwelveMonths_NoDates_ReturnsThisMonthAndTheElevenBefore()
    {
        var w = Wednesday.LastTwelveMonths(null, null, hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2025, 11, 1), new DateOnly(2026, 10, 31), true),
            w
        );
    }

    [Fact]
    public void LastTwelveMonths_ExplicitDates_Win()
    {
        var w = Wednesday.LastTwelveMonths(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), hasParent: false);

        Assert.Equal(
            new DateWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), false),
            w
        );
    }
}
