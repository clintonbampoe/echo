using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class CalendarTests
{
    [Theory]
    [InlineData(2024, "2024-03-31")]
    [InlineData(2025, "2025-04-20")]
    [InlineData(2026, "2026-04-05")]
    public void Easter_IsCorrect(int year, string expected) =>
        Assert.Equal(DateOnly.Parse(expected), Calendar.Easter(year));

    [Theory]
    [InlineData(2026, 10, DayOfWeek.Saturday, 3, "2026-10-17")]
    [InlineData(2026, 10, DayOfWeek.Thursday, 1, "2026-10-01")]
    [InlineData(2026, 10, DayOfWeek.Sunday, 1, "2026-10-04")]
    [InlineData(2026, 2, DayOfWeek.Sunday, 4, "2026-02-22")]
    public void NthWeekday_IsCorrect(int year, int month, DayOfWeek day, int n, string expected) =>
        Assert.Equal(DateOnly.Parse(expected), Calendar.NthWeekday(year, month, day, n));

    [Fact]
    public void OnOrAfter_KeepsTheDateWhenItAlreadyFits()
    {
        var sunday = new DateOnly(2026, 10, 11);

        Assert.Equal(sunday, Calendar.OnOrAfter(sunday, DayOfWeek.Sunday));
        Assert.Equal(
            new DateOnly(2026, 10, 18),
            Calendar.OnOrAfter(sunday.AddDays(1), DayOfWeek.Sunday)
        );
    }

    [Fact]
    public void Every_StepsByWholeWeeksAndIncludesTheEnd()
    {
        var dates = Calendar
            .Every(
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 11, 1),
                DayOfWeek.Sunday,
                everyWeeks: 2
            )
            .ToList();

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 10, 4),
                new DateOnly(2026, 10, 18),
                new DateOnly(2026, 11, 1),
            },
            dates
        );
    }

    [Fact]
    public void MonthlyOn_GivesOneDatePerMonth()
    {
        var dates = Calendar
            .MonthlyOn(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DayOfWeek.Saturday, 3)
            .ToList();

        Assert.Equal(12, dates.Count);
        Assert.All(dates, d => Assert.Equal(DayOfWeek.Saturday, d.DayOfWeek));
    }
}
