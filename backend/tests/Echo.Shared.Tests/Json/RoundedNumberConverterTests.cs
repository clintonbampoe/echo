using System.Text.Json;
using Echo.Shared.Json;

namespace Echo.Shared.Tests.Json;

[Trait("Category", "Unit")]
public class RoundedNumberConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new RoundedDecimalConverter(), new RoundedDoubleConverter() },
    };

    private record Sample(decimal Amount, decimal? Maybe, double Average, int Count);

    [Fact]
    public void Decimals_AreWrittenWithAtMostTwoDecimalPlaces()
    {
        var json = JsonSerializer.Serialize(new Sample(102077525.8349m, 0.004m, 1d, 3), Options);

        Assert.Equal("""{"Amount":102077525.83,"Maybe":0.00,"Average":1,"Count":3}""", json);
    }

    [Fact]
    public void Decimals_RoundHalvesAwayFromZero_SoMoneyDoesNotRoundToTheEvenNeighbour()
    {
        Assert.Equal("0.13", JsonSerializer.Serialize(0.125m, Options));
        Assert.Equal("-0.13", JsonSerializer.Serialize(-0.125m, Options));
    }

    [Fact]
    public void Decimals_ThatAlreadyFit_AreLeftAlone()
    {
        Assert.Equal("12.3", JsonSerializer.Serialize(12.3m, Options));
        Assert.Equal("5", JsonSerializer.Serialize(5m, Options));
    }

    [Fact]
    public void ANullDecimal_StaysNull()
    {
        Assert.Equal("null", JsonSerializer.Serialize((decimal?)null, Options));
    }

    [Fact]
    public void Doubles_AreWrittenWithAtMostTwoDecimalPlaces()
    {
        Assert.Equal("7.35", JsonSerializer.Serialize(7.353529704200845, Options));
        Assert.Equal("49.5", JsonSerializer.Serialize(49.5, Options));
    }

    [Fact]
    public void Numbers_AreStillReadBackExactly()
    {
        var sample = JsonSerializer.Deserialize<Sample>("""{"Amount":12.3456,"Maybe":null,"Average":2.5,"Count":1}""", Options);

        Assert.Equal(new Sample(12.3456m, null, 2.5, 1), sample);
    }
}
