using Echo.Shared.Utilities;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Shared.Tests.Utilities;

[Trait("Category", "Unit")]
public class DateUtilsTests
{
    [Theory]
    [InlineData("2026-09-13", DayOfWeek.Sunday, "2026-09-13")]
    [InlineData("2026-09-14", DayOfWeek.Sunday, "2026-09-13")]
    [InlineData("2026-09-13", DayOfWeek.Monday, "2026-09-07")]
    [InlineData("2026-09-13", DayOfWeek.Friday, "2026-09-11")]
    public void GetFirstDayOfWeek_ReturnsCorrectDate(
        string inputDate,
        DayOfWeek firstDay,
        string expected
    )
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetFirstDayOfWeek(fakeTimeProvider, firstDay);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-13", DayOfWeek.Sunday, "2026-09-19")]
    [InlineData("2026-09-14", DayOfWeek.Sunday, "2026-09-19")]
    [InlineData("2026-09-13", DayOfWeek.Monday, "2026-09-13")]
    [InlineData("2026-09-13", DayOfWeek.Friday, "2026-09-17")]
    public void GetLastDayOfWeek_ReturnsCorrectDate(
        string inputDate,
        DayOfWeek firstDay,
        string expected
    )
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetLastDayOfWeek(fakeTimeProvider, firstDay);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-15", "2026-09-01")]
    [InlineData("2026-01-01", "2026-01-01")]
    [InlineData("2025-12-31", "2025-12-01")]
    public void GetFirstDayOfMonth_ReturnsCorrectDate(string inputDate, string expected)
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetFirstDayOfMonth(fakeTimeProvider);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-15", "2026-09-30")]
    [InlineData("2026-01-01", "2026-01-31")]
    [InlineData("2026-02-15", "2026-02-28")]
    [InlineData("2024-02-15", "2024-02-29")] // leap year
    [InlineData("2025-12-01", "2025-12-31")]
    public void GetLastDayOfMonth_ReturnsCorrectDate(string inputDate, string expected)
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetLastDayOfMonth(fakeTimeProvider);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-15", "2026-01-01")]
    [InlineData("2026-01-01", "2026-01-01")]
    [InlineData("2025-12-31", "2025-01-01")]
    public void GetFirstDayOfYear_ReturnsCorrectDate(string inputDate, string expected)
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetFirstDayOfYear(fakeTimeProvider);

        Assert.Equal(expectedDate, res);
    }

    [Theory]
    [InlineData("2026-09-15", "2026-12-31")]
    [InlineData("2026-01-01", "2026-12-31")]
    [InlineData("2025-12-31", "2025-12-31")]
    public void GetLastDayOfYear_ReturnsCorrectDate(string inputDate, string expected)
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.Parse(inputDate));
        var expectedDate = DateOnly.Parse(expected);

        var res = DateUtils.GetLastDayOfYear(fakeTimeProvider);

        Assert.Equal(expectedDate, res);
    }

    [Fact]
    public void GetDateToday_ReturnsCurrentDate()
    {
        var expected = DateOnly.FromDateTime(DateTime.UtcNow);
        var fakeTimeProvider = new FakeTimeProvider(
            new DateTimeOffset(expected.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
        );

        var res = DateUtils.GetDateToday(fakeTimeProvider);

        Assert.Equal(expected, res);
    }
}
