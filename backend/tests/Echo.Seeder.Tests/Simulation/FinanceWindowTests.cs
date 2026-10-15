using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class FinanceWindowTests
{
    private static readonly DateOnly _today = new(2026, 10, 10);

    [Fact]
    public void For_GoesBackTheRequestedMonths()
    {
        var window = FinanceWindow.For(_today, new DateOnly(2000, 1, 1), months: 36);

        Assert.Equal(new DateOnly(2023, 11, 1), window.FirstMonth);
        Assert.Equal(new DateOnly(2026, 10, 1), window.LastMonth);
        Assert.Equal(36, window.Months().Count());
    }

    [Fact]
    public void For_NeverStartsBeforeTheCongregationBegan()
    {
        var founded = new DateOnly(2026, 3, 14);

        var window = FinanceWindow.For(_today, founded, months: 36);

        Assert.Equal(founded, window.Start);
        Assert.Equal(new DateOnly(2026, 3, 1), window.FirstMonth);
        Assert.Equal(8, window.Months().Count());
    }

    [Fact]
    public void Sundays_AreAllSundays_InsideTheWindow()
    {
        var window = FinanceWindow.For(_today, new DateOnly(2000, 1, 1), months: 12);

        var sundays = window.Sundays().ToList();

        Assert.NotEmpty(sundays);
        Assert.All(sundays, d => Assert.Equal(DayOfWeek.Sunday, d.DayOfWeek));
        Assert.All(
            sundays,
            d => Assert.InRange(d.DayNumber, window.Start.DayNumber, window.Today.DayNumber)
        );
    }
}
