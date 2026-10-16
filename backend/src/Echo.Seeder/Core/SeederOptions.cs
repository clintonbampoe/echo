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
    public DateTimeOffset ReferenceNow { get; init; } = new(2026, 10, 10, 0, 0, 0, TimeSpan.Zero);

    // How many years the big congregation has existed before ReferenceNow.
    // Members, tithes and organizations are spread over this history. Small
    // congregations are younger, between 3 years and HistoryYears - 3.
    [Range(6, 40)]
    public int HistoryYears { get; init; } = 15;

    // Sessions and attendance are recorded for this many weeks before
    // ReferenceNow, and FutureWeeks after it (scheduled, nobody has attended).
    // Attendance is the biggest table: a year is roughly 2 million rows for
    // the default sizes.
    [Range(4, 260)]
    public int AttendanceWeeks { get; init; } = 52;

    [Range(0, 26)]
    public int FutureWeeks { get; init; } = 4;

    // Tithes, offerings, expenses and project contributions are recorded for
    // this many months before ReferenceNow (or since the congregation began,
    // if that is later). A member who gave faithfully for the whole time has
    // this many tithes.
    [Range(6, 240)]
    public int TitheMonths { get; init; } = 36;

    // Events are planned for this many months before ReferenceNow (or since
    // the congregation began, if that is later), plus the next few months.
    [Range(6, 240)]
    public int EventMonths { get; init; } = 36;

    // Big congregation
    [Range(1, 500_000)]
    public int BigMembers { get; init; } = 50_000;

    [Range(0, 50_000)]
    public int BigVisitors { get; init; } = 5_000;

    // Small congregations. SmallMembers and SmallVisitors are the typical
    // size; each congregation lands within SmallSizeSpread of it, so sizes
    // differ (0.6 means anywhere from 40% to 160% of the typical size).
    [Range(1, 20)]
    public int SmallCount { get; init; } = 5;

    [Range(1, 10_000)]
    public int SmallMembers { get; init; } = 200;

    [Range(0, 1_000)]
    public int SmallVisitors { get; init; } = 50;

    [Range(0.0, 0.9)]
    public double SmallSizeSpread { get; init; } = 0.6;
}
