namespace Echo.Shared.Utilities;

public readonly record struct YearMonth(int Year, int Month);

public static class MonthRange
{
    /// <summary>Longest range the monthly summaries will chart.</summary>
    public const int MaxMonths = 24;

    /// <summary>Every calendar month the range touches, oldest first. Empty when from is after to.</summary>
    public static List<YearMonth> Between(DateOnly from, DateOnly to)
    {
        var months = new List<YearMonth>();
        for (var month = new DateOnly(from.Year, from.Month, 1); month <= to; month = month.AddMonths(1))
            months.Add(new YearMonth(month.Year, month.Month));

        return months;
    }

    /// <summary>First day of the first month and last day of the last month. The list must not be empty.</summary>
    public static (DateOnly From, DateOnly To) Bounds(IReadOnlyList<YearMonth> months)
    {
        var first = months[0];
        var last = months[^1];
        return (
            new DateOnly(first.Year, first.Month, 1),
            new DateOnly(last.Year, last.Month, DateTime.DaysInMonth(last.Year, last.Month))
        );
    }

    /// <summary>Why the range cannot be charted, or null when it can.</summary>
    public static string? Problem(IReadOnlyList<YearMonth> months) =>
        months.Count == 0 ? "'from' must not be after 'to'."
        : months.Count > MaxMonths ? $"The range covers {months.Count} months; the maximum is {MaxMonths}."
        : null;
}
