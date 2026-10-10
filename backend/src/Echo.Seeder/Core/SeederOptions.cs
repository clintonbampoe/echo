using System.ComponentModel.DataAnnotations;

namespace Echo.Seeder.Core;

public sealed class SeederOptions
{
    public const string Section = "Seeder";

    [Range(1, int.MaxValue)]
    public int Seed { get; init; } = 42;

    public bool Reset { get; init; } = false;

    [Range(100, 10_000)]
    public int BatchSize { get; init; } = 2_000;

    // Every generated timestamp is relative to this instant.
    // Fixed so identical data is produced on every run regardless of when it runs.
    public DateTimeOffset ReferenceNow { get; init; } = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Big congregation
    [Range(1, 500_000)]
    public int BigMembers { get; init; } = 50_000;

    [Range(0, 50_000)]
    public int BigVisitors { get; init; } = 5_000;

    // Small congregations
    [Range(1, 20)]
    public int SmallCount { get; init; } = 5;

    [Range(1, 10_000)]
    public int SmallMembers { get; init; } = 200;

    [Range(0, 1_000)]
    public int SmallVisitors { get; init; } = 50;
}
