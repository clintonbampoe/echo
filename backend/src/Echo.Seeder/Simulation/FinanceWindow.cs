namespace Echo.Seeder.Simulation;

/// <summary>
/// The stretch of time the money data covers: from the first day of data to
/// today. Tithes, offerings, expenses and project contributions all use the
/// same window, so the books agree with each other.
/// </summary>
public sealed record FinanceWindow(DateOnly Start, DateOnly Today)
{
    /// <summary>The first day of the month the data starts in.</summary>
    public DateOnly FirstMonth => new(Start.Year, Start.Month, 1);

    /// <summary>The first day of the current month.</summary>
    public DateOnly LastMonth => new(Today.Year, Today.Month, 1);

    /// <summary>
    /// The window for a congregation: <paramref name="months"/> months back from
    /// today, but never before the congregation began.
    /// </summary>
    public static FinanceWindow For(DateOnly today, DateOnly founded, int months)
    {
        var monthsBack = new DateOnly(today.Year, today.Month, 1).AddMonths(-(months - 1));
        return new FinanceWindow(monthsBack > founded ? monthsBack : founded, today);
    }

    /// <summary>The first day of every month in the window, oldest first.</summary>
    public IEnumerable<DateOnly> Months()
    {
        for (var month = FirstMonth; month <= LastMonth; month = month.AddMonths(1))
            yield return month;
    }

    /// <summary>Every Sunday in the window.</summary>
    public IEnumerable<DateOnly> Sundays() => Calendar.Every(Start, Today, DayOfWeek.Sunday);
}
