using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Core;

[Trait("Category", "Unit")]
public class FoundingTests
{
    private static readonly DateOnly _today = new(2026, 10, 10);
    private static readonly SeederOptions _options = new();

    [Fact]
    public void BigCongregation_IsAsOldAsHistoryYears()
    {
        var founded = Founding.FoundedOn(Rng.ForPhase(1, "Setup"), _options, _today, isBig: true);

        var years = (_today.DayNumber - founded.DayNumber) / 365.25;

        Assert.InRange(years, _options.HistoryYears - 0.01, _options.HistoryYears + 0.01);
    }

    [Fact]
    public void SmallCongregations_AreYoungerThanTheBigOne_ButAtLeastThreeYearsOld()
    {
        var big = Founding.FoundedOn(Rng.ForPhase(1, "Setup"), _options, _today, isBig: true);

        for (var seed = 1; seed <= 200; seed++)
        {
            var founded = Founding.FoundedOn(
                Rng.ForPhase(seed, "Setup"),
                _options,
                _today,
                isBig: false
            );

            Assert.True(founded > big);
            Assert.True(founded <= _today.AddYears(-3).AddDays(1));
        }
    }
}
