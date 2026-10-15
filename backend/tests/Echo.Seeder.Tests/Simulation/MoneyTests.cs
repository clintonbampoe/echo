using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class MoneyTests
{
    [Theory]
    [InlineData(87.43, 85)]
    [InlineData(12.0, 10)]
    [InlineData(3.0, 10)]
    [InlineData(243.0, 240)]
    [InlineData(1_234.0, 1_250)]
    [InlineData(5_432.0, 5_400)]
    public void RoundLikeAPerson_UsesBiggerStepsForBiggerAmounts(double value, decimal expected) =>
        Assert.Equal(expected, Money.RoundLikeAPerson(value));

    [Theory]
    [InlineData(723_418.0, 720_000)]
    [InlineData(14.2, 14)]
    [InlineData(4.0, 10)]
    [InlineData(1_551.0, 1_600)]
    public void RoundNicely_KeepsTwoSignificantFigures(double value, decimal expected) =>
        Assert.Equal(expected, Money.RoundNicely(value));

    [Fact]
    public void Cents_RoundsToTwoDecimals() => Assert.Equal(12.35m, Money.Cents(12.3456));
}
