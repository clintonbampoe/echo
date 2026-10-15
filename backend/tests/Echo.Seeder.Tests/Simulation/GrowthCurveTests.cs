using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class GrowthCurveTests
{
    private static readonly DateOnly _founded = new(2011, 10, 10);
    private static readonly DateOnly _today = new(2026, 10, 10);

    [Fact]
    public void SampleJoinDate_StaysBetweenFoundedAndToday()
    {
        var curve = new GrowthCurve(_founded, _today, 0.15);
        var rng = Rng.ForPhase(7, "Growth");

        for (var i = 0; i < 2_000; i++)
        {
            var date = curve.SampleJoinDate(rng);

            Assert.InRange(date.DayNumber, _founded.DayNumber, _today.DayNumber);
        }
    }

    [Fact]
    public void SampleJoinDate_MostPeopleJoinedRecently()
    {
        var curve = new GrowthCurve(_founded, _today, 0.15);
        var rng = Rng.ForPhase(7, "Growth");
        var dates = Enumerable.Range(0, 5_000).Select(_ => curve.SampleJoinDate(rng)).ToList();

        var firstFiveYears = dates.Count(d => d < _founded.AddYears(5));
        var lastFiveYears = dates.Count(d => d >= _today.AddYears(-5));

        Assert.True(lastFiveYears > firstFiveYears * 2);
    }

    [Fact]
    public void SampleJoinDate_IsRepeatable()
    {
        var curve = new GrowthCurve(_founded, _today, 0.15);
        var a = Rng.ForPhase(7, "Growth");
        var b = Rng.ForPhase(7, "Growth");

        for (var i = 0; i < 50; i++)
            Assert.Equal(curve.SampleJoinDate(a), curve.SampleJoinDate(b));
    }

    [Fact]
    public void Constructor_RejectsBadInput()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GrowthCurve(_today, _founded, 0.15));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GrowthCurve(_founded, _today, 0));
    }
}
