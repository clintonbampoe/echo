using Echo.Application.Utilities;
using Xunit;

namespace Echo.Application.Tests.Utilities;

[Trait("Category", "Unit")]
public class DateUtilsTests
{
    [Theory]
    [InlineData("2026-09-13", DayOfWeek.Sunday, "2026-09-13")] // Sunday is first day, today is Sunday
    [InlineData("2026-09-14", DayOfWeek.Sunday, "2026-09-13")] // Sunday is first day, today is Monday
    [InlineData("2026-09-13", DayOfWeek.Monday, "2026-09-07")] // Monday is first day, today is Sunday
    [InlineData("2026-09-13", DayOfWeek.Friday, "2026-09-11")] // Friday is first day, today is Sunday
    public void GetFirstDayOfWeek_ReturnsCorrectDate(
        string inputDate,
        DayOfWeek firstDay,
        string expected
    )
    {
        var date = DateTime.Parse(inputDate);
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetFirstDayOfWeek(date, firstDay);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-15", "2026-09-01")]
    [InlineData("2026-01-01", "2026-01-01")]
    [InlineData("2025-12-31", "2025-12-01")]
    public void GetFirstDayOfMonth_ReturnsCorrectDate(string inputDate, string expected)
    {
        var date = DateTime.Parse(inputDate);
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetFirstDayOfMonth(date);

        Assert.Equal(expectedDate, res);
    }

    [Fact]
    public void GetDateToday_ReturnsCurrentDate()
    {
        var res = DateUtils.GetDateToday();
        var expected = DateOnly.FromDateTime(DateTime.UtcNow);

        // Allow for a small discrepancy if test runs exactly at midnight
        Assert.Equal(expected.Year, res.Year);
        Assert.Equal(expected.Month, res.Month);
        Assert.Equal(expected.Day, res.Day);
    }
}

