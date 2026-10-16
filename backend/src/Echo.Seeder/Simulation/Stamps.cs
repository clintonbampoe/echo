using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Turns a calendar date into the instant a record was created on that day.
/// </summary>
public static class Stamps
{
    /// <summary>
    /// A UTC instant on <paramref name="date"/> at a random time between the
    /// two hours. Never later than <paramref name="now"/>.
    /// </summary>
    public static DateTime On(
        DateOnly date,
        Rng rng,
        DateTime now,
        int fromHour = 8,
        int toHour = 16
    )
    {
        var at = date.ToDateTime(
            new TimeOnly(rng.Next(fromHour, toHour), rng.Next(60)),
            DateTimeKind.Utc
        );
        return at > now ? now : at;
    }
}
