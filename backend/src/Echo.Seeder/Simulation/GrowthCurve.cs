using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// When people joined. A congregation grows roughly exponentially: more people
/// join in a recent year than in its first year, so most of today's members
/// joined in the last few years. Joins also bunch around January, Easter and
/// December, and most are recorded on a Sunday.
/// </summary>
public sealed class GrowthCurve
{
    // Index 0 is January.
    private static readonly double[] _monthWeights =
    [
        1.3,
        0.9,
        0.9,
        1.25,
        0.9,
        0.9,
        0.8,
        0.9,
        1.0,
        1.0,
        1.0,
        1.3,
    ];

    private static readonly double _maxWeight = _monthWeights.Max();

    private readonly DateOnly _today;
    private readonly double _annualGrowth;
    private readonly double _yearsOfHistory;

    public DateOnly Founded { get; }

    /// <param name="founded">The day the congregation started.</param>
    /// <param name="today">The day the history ends.</param>
    /// <param name="annualGrowth">
    /// Exponential rate. 0.15 means the join rate grows about 16% a year.
    /// </param>
    public GrowthCurve(DateOnly founded, DateOnly today, double annualGrowth)
    {
        if (today <= founded)
            throw new ArgumentOutOfRangeException(nameof(founded), "Founded must be before today.");
        if (annualGrowth <= 0)
            throw new ArgumentOutOfRangeException(nameof(annualGrowth), "Growth must be positive.");

        Founded = founded;
        _today = today;
        _annualGrowth = annualGrowth;
        _yearsOfHistory = (today.DayNumber - founded.DayNumber) / 365.25;
    }

    public DateOnly SampleJoinDate(Rng rng)
    {
        var span = Math.Exp(_annualGrowth * _yearsOfHistory) - 1.0;

        while (true)
        {
            // Inverse of the cumulative join rate: years since founding.
            var years = Math.Log(1.0 + rng.NextDouble() * span) / _annualGrowth;
            var date = Founded.AddDays((int)(years * 365.25));

            if (date > _today)
                continue;

            // Accept in proportion to the month's weight.
            if (rng.NextDouble() * _maxWeight > _monthWeights[date.Month - 1])
                continue;

            return OnASunday(date, rng);
        }
    }

    private DateOnly OnASunday(DateOnly date, Rng rng)
    {
        if (!rng.Chance(0.8))
            return date;

        var sunday = date.AddDays(-(int)date.DayOfWeek);
        return sunday >= Founded ? sunday : date;
    }
}
