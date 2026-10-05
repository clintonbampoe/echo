namespace Echo.Shared.Utilities;

public static class DateUtils
{
    public static DateOnly GetFirstDayOfWeek(
        TimeProvider timeProvider,
        DayOfWeek firstDayOfWeek = DayOfWeek.Sunday
    )
    {
        DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
        int diff = ((int)today.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        return today.AddDays(-diff);
    }

    public static DateOnly GetLastDayOfWeek(
        TimeProvider timeProvider,
        DayOfWeek firstDayOfWeek = DayOfWeek.Sunday
    )
    {
        return GetFirstDayOfWeek(timeProvider, firstDayOfWeek).AddDays(6);
    }

    public static DateOnly GetFirstDayOfMonth(TimeProvider timeProvider)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        return new DateOnly(today.Year, today.Month, 1);
    }

    public static DateOnly GetLastDayOfMonth(TimeProvider timeProvider)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        return new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
    }

    public static DateOnly GetFirstDayOfYear(TimeProvider timeProvider)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        return new DateOnly(today.Year, 1, 1);
    }

    public static DateOnly GetLastDayOfYear(TimeProvider timeProvider)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        return new DateOnly(today.Year, 12, 31);
    }

    public static DateOnly GetDateToday(TimeProvider timeProvider)
    {
        return DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
    }
}
