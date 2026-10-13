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
    public void MaxMonths_IsTwoYears()
    {
        Assert.Equal(24, MonthRange.MaxMonths);
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

    [Fact]
    public void Problem_WhenFromIsAfterTo_SaysSo()
    {
        var months = MonthRange.Between(new DateOnly(2026, 5, 1), new DateOnly(2026, 1, 1));

        Assert.Contains("must not be after", MonthRange.Problem(months));
    }

    [Fact]
    public void Problem_WhenTheRangeIsTooLong_NamesTheLimit()
    {
        var months = MonthRange.Between(new DateOnly(2024, 1, 1), new DateOnly(2026, 1, 1));

        Assert.Contains("maximum is 24", MonthRange.Problem(months));
    }

    [Fact]
    public void Problem_WithinTheLimit_IsNull()
    {
        var months = MonthRange.Between(new DateOnly(2025, 11, 1), new DateOnly(2026, 10, 31));

        Assert.Null(MonthRange.Problem(months));
    }
}
