using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Tests.Simulation;

[Trait("Category", "Unit")]
public class GivingFactoryTests
{
    [Fact]
    public void Create_ChildrenDoNotTithe()
    {
        var giving = GivingFactory.Create(Rng.ForPhase(1, "Giving"), age: 12, 0.9, IncomeBand.High);

        Assert.Equal(GivingStyle.None, giving.Style);
        Assert.Equal(0m, giving.MonthlyAmount);
    }

    [Fact]
    public void Create_IsRepeatable()
    {
        var a = GivingFactory.Create(Rng.ForPhase(1, "Giving"), 35, 0.7, IncomeBand.Middle);
        var b = GivingFactory.Create(Rng.ForPhase(1, "Giving"), 35, 0.7, IncomeBand.Middle);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Create_CommittedAdultsAreFaithfulMoreOftenThanUncommittedOnes()
    {
        var rng = Rng.ForPhase(1, "Giving");

        int Faithful(double commitment) =>
            Enumerable
                .Range(0, 1_000)
                .Count(_ =>
                    GivingFactory.Create(rng, 40, commitment, IncomeBand.Middle).Style
                    == GivingStyle.Faithful
                );

        Assert.True(Faithful(0.95) > Faithful(0.1));
    }

    [Fact]
    public void PickBand_GivesMostPeopleTheLowAndMiddleBands()
    {
        var rng = Rng.ForPhase(1, "Giving");
        var bands = Enumerable.Range(0, 2_000).Select(_ => GivingFactory.PickBand(rng)).ToList();

        Assert.True(
            bands.Count(b => b == IncomeBand.High) < bands.Count(b => b != IncomeBand.High)
        );
    }
}
