namespace Echo.Application.Utilities;

public static class DateUtils
{
    public static DateOnly GetFirstDayOfWeek(
        DateTime dateTimeToday,
        DayOfWeek firstDayOfWeek = DayOfWeek.Sunday
    )
    {
        DateOnly today = DateOnly.FromDateTime(dateTimeToday);
        int diff = ((int)today.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
        return today.AddDays(-diff);
    }

    public static DateOnly GetFirstDayOfMonth(DateTime dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, 1);
    }

    public static DateOnly GetDateToday()
    {
        return DateOnly.FromDateTime(TimeProvider.System.GetUtcNow().DateTime);
    }
}
