using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Decides when a congregation began. Everything else (members, ministries,
/// tithes, assets) is placed after that day.
/// </summary>
public static class Founding
{
    /// <summary>
    /// The big congregation is as old as HistoryYears. A small one is between
    /// 3 years and HistoryYears - 3 old, with a random day of the year so they
    /// do not all start on the same date.
    /// </summary>
    public static DateOnly FoundedOn(Rng rng, SeederOptions options, DateOnly today, bool isBig)
    {
        if (isBig)
            return today.AddDays(-(int)(options.HistoryYears * 365.25));

        var years = rng.Next(3, options.HistoryYears - 2);
        return today.AddDays(-(int)(years * 365.25) - rng.Next(0, 365));
    }
}
