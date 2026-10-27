using Echo.Shared.Utilities;

namespace Echo.Shared.Tests.Utilities;

[Trait("Category", "Unit")]
public class MonthRangeTests
{
    [Fact]
    public void Between_WithinOneYear_ListsEveryMonthInclusive()
    {
        var res = MonthRange.Between(new DateOnly(2026, 1, 15), new DateOnly(2026, 3, 2));

        Assert.Equal([new YearMonth(2026, 1), new YearMonth(2026, 2), new YearMonth(2026, 3)], res);
    }

    [Fact]
    public void Between_AcrossAYearBoundary_ContinuesIntoTheNextYear()
    {
        var res = MonthRange.Between(new DateOnly(2025, 11, 30), new DateOnly(2026, 2, 1));

        Assert.Equal(
            [new YearMonth(2025, 11), new YearMonth(2025, 12), new YearMonth(2026, 1), new YearMonth(2026, 2)],
            res
        );
    }

    [Fact]
    public void Between_SameMonth_ReturnsThatMonthOnly()
    {
        var res = MonthRange.Between(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        Assert.Equal([new YearMonth(2026, 5)], res);
    }

    [Fact]
    public void Between_FromAfterTo_IsEmpty()
    {
        Assert.Empty(MonthRange.Between(new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1)));
    }

    [Fact]
    public void MaxMonths_IsOneYear_AndTheDefaultIsTheMax()
    {
        Assert.Equal(12, MonthRange.MaxMonths);
        Assert.Equal(MonthRange.MaxMonths, MonthRange.DefaultMonths);
    }

    [Fact]
    public void Bounds_CoverTheWholeFirstAndLastMonth()
    {
        var months = MonthRange.Between(new DateOnly(2026, 1, 15), new DateOnly(2026, 3, 10));

        var (from, to) = MonthRange.Bounds(months);

        Assert.Equal((new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31)), (from, to));
    }

    [Fact]
    public void Bounds_UseTheRealLastDayOfAFebruaryInALeapYear()
    {
        var months = MonthRange.Between(new DateOnly(2028, 2, 3), new DateOnly(2028, 2, 3));

        Assert.Equal(new DateOnly(2028, 2, 29), MonthRange.Bounds(months).To);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(13)]
    public void Problem_WhenMonthsIsOutsideOneToTwelve_NamesTheLimits(int months)
    {
        Assert.Contains("between 1 and 12", MonthRange.Problem(months));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Problem_WhenMonthsIsInRange_IsNone(int months)
    {
        Assert.Null(MonthRange.Problem(months));
    }

    [Fact]
    public void Ending_ListsTheMonthsOldestFirst_EndingWithTheMonthOfTheDay()
    {
        var res = MonthRange.Ending(new DateOnly(2026, 10, 27), 3);

        Assert.Equal([new YearMonth(2026, 8), new YearMonth(2026, 9), new YearMonth(2026, 10)], res);
    }

    [Fact]
    public void Ending_ReachesBackAcrossAYearBoundary()
    {
        var res = MonthRange.Ending(new DateOnly(2026, 2, 1), 12);

        Assert.Equal(new YearMonth(2025, 3), res[0]);
        Assert.Equal(new YearMonth(2026, 2), res[^1]);
        Assert.Equal(12, res.Count);
    }

    private sealed record Row(int Year, int Month, int Count);

    [Fact]
    public void Fill_ReturnsEveryMonthInOrder_WithAnEmptyRowWhereThereIsNone()
    {
        var months = MonthRange.Between(new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 1));
        var rows = new[] { new Row(2026, 3, 5), new Row(2026, 1, 2) };

        var res = MonthRange.Fill(months, rows, r => new YearMonth(r.Year, r.Month), m => new Row(m.Year, m.Month, 0));

        Assert.Equal([2, 0, 5, 0], res.Select(r => r.Count));
        Assert.Equal([1, 2, 3, 4], res.Select(r => r.Month));
    }

    [Fact]
    public void Fill_LeavesOutRowsOutsideTheRange()
    {
        var months = MonthRange.Between(new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));
        var rows = new[] { new Row(2026, 1, 9), new Row(2026, 2, 4) };

        var res = MonthRange.Fill(months, rows, r => new YearMonth(r.Year, r.Month), m => new Row(m.Year, m.Month, 0));

        Assert.Equal([new Row(2026, 2, 4)], res);
    }
}
