using Echo.Seeder.Core;

namespace Echo.Seeder.Tests.Core;

[Trait("Category", "Unit")]
public class RngTests
{
    [Fact]
    public void SameSeedPhaseAndScope_GiveTheSameSequence()
    {
        var a = Rng.ForPhase(42, "People", "Grace Chapel");
        var b = Rng.ForPhase(42, "People", "Grace Chapel");

        for (var i = 0; i < 20; i++)
            Assert.Equal(a.Next(1_000_000), b.Next(1_000_000));
    }

    [Fact]
    public void DifferentScope_GivesADifferentSequence()
    {
        var a = Rng.ForPhase(42, "People", "Grace Chapel");
        var b = Rng.ForPhase(42, "People", "Zoe Chapel");

        var same = Enumerable.Range(0, 20).Count(_ => a.Next(1_000_000) == b.Next(1_000_000));

        Assert.True(same < 3);
    }

    [Fact]
    public void DifferentSeed_GivesADifferentSequence()
    {
        var a = Rng.ForPhase(1, "People");
        var b = Rng.ForPhase(2, "People");

        Assert.NotEqual(a.NewGuid(), b.NewGuid());
    }

    [Fact]
    public void Fork_DoesNotConsumeNumbersFromTheParent()
    {
        var withFork = Rng.ForPhase(42, "Finances");
        var without = Rng.ForPhase(42, "Finances");

        withFork.Fork("tithes").Next(100);

        Assert.Equal(without.Next(1_000_000), withFork.Next(1_000_000));
    }

    [Fact]
    public void NewGuid_IsRepeatableAndUnique()
    {
        var a = Rng.ForPhase(42, "Ids");
        var b = Rng.ForPhase(42, "Ids");

        var first = Enumerable.Range(0, 100).Select(_ => a.NewGuid()).ToList();
        var second = Enumerable.Range(0, 100).Select(_ => b.NewGuid()).ToList();

        Assert.Equal(first, second);
        Assert.Equal(100, first.Distinct().Count());
    }
}
