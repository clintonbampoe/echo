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

    public static DateOnly GetFirstDayOfMonth(TimeProvider timeProvider)
    {
        var today = timeProvider.GetUtcNow().DateTime;
        return new DateOnly(today.Year, today.Month, 1);
    }

    public static DateOnly GetDateToday(TimeProvider timeProvider)
    {
        return DateOnly.FromDateTime(timeProvider.GetUtcNow().DateTime);
    }
}
