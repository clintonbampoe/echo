namespace Echo.Seeder.Core;

/// <summary>
/// A thin wrapper around System.Random with a fixed seed.
/// <br/><br/>
/// One Rng instance is created per phase, seeded from the global seed plus
/// the phase name.
/// </summary>
public sealed class Rng
{
    private readonly Random _random;

    private Rng(int seed) => _random = new Random(seed);

    /// <summary>
    /// Creates an Rng for a named phase. The phase name is hashed together
    /// with the global seed so each phase gets an independent sequence.
    /// </summary>
    public static Rng ForPhase(int globalSeed, string phaseName)
    {
        var combined = HashCode.Combine(globalSeed, phaseName);
        return new Rng(combined);
    }

    /// <summary>Returns a non-negative integer in [0, maxExclusive).</summary>
    public int Next(int maxExclusive) => _random.Next(maxExclusive);

    /// <summary>Returns an integer in [min, maxExclusive).</summary>
    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

    /// <summary>Returns a double in [0.0, 1.0).</summary>
    public double NextDouble() => _random.NextDouble();

    /// <summary>Returns true with the given probability (0.0–1.0).</summary>
    public bool Chance(double probability) => _random.NextDouble() < probability;

    /// <summary>Picks a random element from a list.</summary>
    public T Pick<T>(IReadOnlyList<T> items)
    {
        if (items.Count == 0)
            throw new ArgumentException("Cannot pick from an empty list.", nameof(items));

        return items[_random.Next(items.Count)];
    }

    /// <summary>
    /// Picks a random element according to weights.
    /// Each entry is (value, weight) where weight is a relative probability.
    /// </summary>
    public T Weighted<T>(IReadOnlyList<(T Value, double Weight)> options)
    {
        if (options.Count == 0)
            throw new ArgumentException("Options list is empty.", nameof(options));

        var total = 0.0;
        foreach (var (_, w) in options)
            total += w;

        var roll = _random.NextDouble() * total;
        var cumulative = 0.0;

        foreach (var (value, weight) in options)
        {
            cumulative += weight;
            if (roll < cumulative)
                return value;
        }

        return options[^1].Value;
    }

    /// <summary>
    /// Returns a new list with elements in a random order (Fisher-Yates).
    /// Does not modify the original.
    /// </summary>
    public List<T> Shuffle<T>(IReadOnlyList<T> source)
    {
        var copy = new List<T>(source);
        for (var i = copy.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy;
    }

    /// <summary>
    /// Picks a random subset of <paramref name="count"/> elements from
    /// <paramref name="source"/> without replacement.
    /// </summary>
    public List<T> PickSubset<T>(IReadOnlyList<T> source, int count)
    {
        var shuffled = Shuffle(source);
        return [.. shuffled.Take(Math.Min(count, shuffled.Count))];
    }

    /// <summary>
    /// Picks <paramref name="count"/> distinct elements from the virtual
    /// concatenation of <paramref name="head"/> and <paramref name="tail"/>.
    ///
    /// <br/><br/>
    ///
    /// Uses rejection sampling over integer indices. Fast when count is much
    /// smaller than the combined pool size, which is the case in every caller.
    /// Does not allocate a copy of the pools.
    /// </summary>
    public List<T> PickFromPair<T>(IReadOnlyList<T> head, IReadOnlyList<T> tail, int count)
    {
        var total = head.Count + tail.Count;

        if (count >= total)
        {
            var all = new List<T>(total);
            all.AddRange(head);
            all.AddRange(tail);
            return all;
        }

        var picked = new List<T>(count);
        var seen = new HashSet<int>(count);

        while (picked.Count < count)
        {
            var idx = _random.Next(total);
            if (!seen.Add(idx))
                continue;

            picked.Add(idx < head.Count ? head[idx] : tail[idx - head.Count]);
        }

        return picked;
    }

    /// <summary>
    /// Returns a DateTime between start (inclusive) and end (exclusive).
    /// </summary>
    public DateTime Between(DateTime start, DateTime end)
    {
        var range = (end - start).TotalSeconds;
        return start.AddSeconds(_random.NextDouble() * range);
    }

    /// <summary>Returns a DateOnly between start and end (both inclusive).</summary>
    public DateOnly BetweenDates(DateOnly start, DateOnly end)
    {
        var range = end.DayNumber - start.DayNumber;
        return start.AddDays(_random.Next(range + 1));
    }

    /// <summary>
    /// Returns a date within the past <paramref name="maxDaysAgo"/> days,
    /// relative to <paramref name="now"/>.
    /// </summary>
    public DateOnly RecentDate(DateTime now, int maxDaysAgo)
    {
        return DateOnly.FromDateTime(now.AddDays(-_random.Next(0, maxDaysAgo)));
    }
}
