using System.Text.Json;
using System.Text.Json.Serialization;

namespace Echo.Shared.Json;

/// <summary>
/// Writes money and other decimals with at most two decimal places, whatever the database
/// computed (an average of many amounts has many digits nobody can use). Reading is unchanged.
/// Halves round away from zero, the way money is rounded by hand.
/// </summary>
public sealed class RoundedDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => reader.GetDecimal();

    public override void Write(
        Utf8JsonWriter writer,
        decimal value,
        JsonSerializerOptions options
    ) => writer.WriteNumberValue(Math.Round(value, 2, MidpointRounding.AwayFromZero));
}

/// <summary>Writes averages and other doubles with at most two decimal places. Reading is unchanged.</summary>
public sealed class RoundedDoubleConverter : JsonConverter<double>
{
    public override double Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => reader.GetDouble();

    public override void Write(
        Utf8JsonWriter writer,
        double value,
        JsonSerializerOptions options
    ) => writer.WriteNumberValue(Math.Round(value, 2, MidpointRounding.AwayFromZero));
}
