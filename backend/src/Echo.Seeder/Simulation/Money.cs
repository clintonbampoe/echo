namespace Echo.Seeder.Simulation;

/// <summary>
/// Rounds amounts the way people and budgets do, so the data has 150 and
/// 12,000 in it instead of 87.43.
/// </summary>
public static class Money
{
    /// <summary>An amount to the cent.</summary>
    public static decimal Cents(double value) => Math.Round((decimal)value, 2);

    /// <summary>
    /// What a person gives: 50, 100, 120 or 500. Steps get bigger as the amount
    /// does, and nobody gives less than 10.
    /// </summary>
    public static decimal RoundLikeAPerson(double value)
    {
        var step = value switch
        {
            < 100 => 5.0,
            < 500 => 10.0,
            < 2000 => 50.0,
            _ => 100.0,
        };

        return (decimal)Math.Max(10.0, Math.Round(value / step) * step);
    }

    /// <summary>
    /// An amount a budget would use: two significant figures, so 723,418
    /// becomes 720,000 and 14.2 becomes 14. Never below 10.
    /// </summary>
    public static decimal RoundNicely(double value)
    {
        if (value < 10.0)
            return 10m;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)) - 1);
        return (decimal)(Math.Round(value / magnitude) * magnitude);
    }
}
