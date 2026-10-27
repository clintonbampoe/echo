namespace Echo.Shared.Utilities;

public readonly record struct YearMonth(int Year, int Month);

public static class MonthRange
{
    /// <summary>Most months a monthly summary will return.</summary>
    public const int MaxMonths = 12;

    /// <summary>How many months a monthly summary returns when not asked.</summary>
    public const int DefaultMonths = 12;

    /// <summary>Every calendar month the range touches, oldest first. Empty when from is after to.</summary>
    public static List<YearMonth> Between(DateOnly from, DateOnly to)
    {
        var months = new List<YearMonth>();
        for (var month = new DateOnly(from.Year, from.Month, 1); month <= to; month = month.AddMonths(1))
            months.Add(new YearMonth(month.Year, month.Month));

        return months;
    }

    /// <summary>
    /// One item per month, oldest first: the row for that month, or an empty one when there is none,
    /// so a chart has no gaps. Rows outside the months are left out.
    /// </summary>
    public static List<T> Fill<T>(
        IReadOnlyList<YearMonth> months,
        IEnumerable<T> rows,
        Func<T, YearMonth> monthOf,
        Func<YearMonth, T> empty
    )
        where T : class
    {
        var byMonth = rows.ToDictionary(monthOf);
        return [.. months.Select(m => byMonth.GetValueOrDefault(m) ?? empty(m))];
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

    /// <summary>
    /// The given number of months, oldest first, ending with the month of <paramref name="day"/>.
    /// </summary>
    public static List<YearMonth> Ending(DateOnly day, int months)
    {
        var last = new DateOnly(day.Year, day.Month, 1);
        return Between(last.AddMonths(-(months - 1)), last);
    }

    /// <summary>Why the number of months cannot be charted, or null when it can.</summary>
    public static string? Problem(int months) =>
        months is < 1 or > MaxMonths ? $"'months' must be between 1 and {MaxMonths}." : null;
}
