namespace Echo.Seeder.Core;

/// <summary>
/// A thin wrapper around System.Random with a fixed seed.
/// <br/><br/>
/// Every phase (and every congregation inside a phase) gets its own Rng, so
/// the data one phase produces never shifts when another changes.
/// </summary>
public sealed class Rng
{
    private readonly Random _random;
    private readonly int _seed;

    private Rng(int seed)
    {
        _seed = seed;
        _random = new Random(seed);
    }

    /// <summary>
    /// Creates an Rng for a named phase, optionally narrowed to a scope such
    /// as a congregation name. The same inputs always give the same sequence.
    /// </summary>
    public static Rng ForPhase(int globalSeed, string phaseName, string? scope = null) =>
        new(StableSeed(globalSeed, phaseName, scope));

    /// <summary>
    /// A new independent Rng derived from this one's seed and a name.
    /// Does not consume any numbers from this Rng.
    /// </summary>
    public Rng Fork(string name) => new(StableSeed(_seed, name, null));

    // FNV-1a over the seed bytes and the names. string.GetHashCode and
    // HashCode.Combine are randomised per process in .NET, so they would give
    // different data on every run.
    private static int StableSeed(int globalSeed, string phase, string? scope)
    {
        unchecked
        {
            var hash = 2166136261u;

            for (var shift = 0; shift < 32; shift += 8)
                hash = (hash ^ (byte)(globalSeed >> shift)) * 16777619u;

            foreach (var c in phase)
                hash = (hash ^ c) * 16777619u;

            if (scope is not null)
            {
                hash = (hash ^ 0x1F) * 16777619u;
                foreach (var c in scope)
                    hash = (hash ^ c) * 16777619u;
            }

            return (int)(hash & 0x7FFFFFFF);
        }
    }

    /// <summary>A random v4-shaped Guid that is the same on every run.</summary>
    public Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        _random.NextBytes(bytes);
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }

    /// <summary>Returns a non-negative integer in [0, maxExclusive).</summary>
    public int Next(int maxExclusive) => _random.Next(maxExclusive);

    /// <summary>Returns an integer in [min, maxExclusive).</summary>
    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

    /// <summary>Returns a double in [0.0, 1.0).</summary>
    public double NextDouble() => _random.NextDouble();

    /// <summary>Returns a double in [min, max).</summary>
    public double Range(double min, double max) => min + _random.NextDouble() * (max - min);

    /// <summary>Returns true with the given probability (0.0–1.0).</summary>
    public bool Chance(double probability) => _random.NextDouble() < probability;

    /// <summary>A normally distributed value (Box-Muller).</summary>
    public double Gaussian(double mean, double standardDeviation)
    {
        var u1 = 1.0 - _random.NextDouble();
        var u2 = _random.NextDouble();
        var standard = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + standard * standardDeviation;
    }

    /// <summary>
    /// A log-normally distributed value: always positive, most values near
    /// <paramref name="median"/>, with a long tail of large ones. This is the
    /// shape of money amounts.
    /// </summary>
    public double LogNormal(double median, double sigma) => median * Math.Exp(Gaussian(0, sigma));

    /// <summary>An exponentially distributed value with the given mean.</summary>
    public double Exponential(double mean) => -mean * Math.Log(1.0 - _random.NextDouble());

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
