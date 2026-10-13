namespace Echo.Shared.Utilities;

/// <summary>
/// Decides the default date window for list and summary requests.
/// Each method returns null when the request is scoped to a parent record
/// (hasParent) and no dates were sent: the caller then applies no date filter.
/// </summary>
public sealed class DateWindows(TimeProvider timeProvider)
{
    public DateWindow? ThisWeekToDate(DateOnly? from, DateOnly? to, bool hasParent) =>
        Resolve(
            from,
            to,
            hasParent,
            DateUtils.GetFirstDayOfWeek(timeProvider),
            DateUtils.GetDateToday(timeProvider)
        );

    public DateWindow? ThisWeek(DateOnly? from, DateOnly? to, bool hasParent) =>
        Resolve(
            from,
            to,
            hasParent,
            DateUtils.GetFirstDayOfWeek(timeProvider),
            DateUtils.GetLastDayOfWeek(timeProvider)
        );

    public DateWindow? ThisMonthToDate(DateOnly? from, DateOnly? to, bool hasParent) =>
        Resolve(
            from,
            to,
            hasParent,
            DateUtils.GetFirstDayOfMonth(timeProvider),
            DateUtils.GetDateToday(timeProvider)
        );

    public DateWindow? ThisYear(DateOnly? from, DateOnly? to, bool hasParent) =>
        Resolve(
            from,
            to,
            hasParent,
            DateUtils.GetFirstDayOfYear(timeProvider),
            DateUtils.GetLastDayOfYear(timeProvider)
        );

    public DateWindow? LastTwelveMonths(DateOnly? from, DateOnly? to, bool hasParent) =>
        Resolve(
            from,
            to,
            hasParent,
            DateUtils.GetFirstDayOfMonth(timeProvider).AddMonths(-11),
            DateUtils.GetLastDayOfMonth(timeProvider)
        );

    private static DateWindow? Resolve(
        DateOnly? from,
        DateOnly? to,
        bool hasParent,
        DateOnly defaultFrom,
        DateOnly defaultTo
    )
    {
        if (from is not null || to is not null)
            return new DateWindow(from ?? defaultFrom, to ?? defaultTo, IsDefault: false);

        if (hasParent)
            return null;

        return new DateWindow(defaultFrom, defaultTo, IsDefault: true);
    }
}
