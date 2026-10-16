namespace Echo.Seeder.Simulation;

/// <summary>
/// Calendar arithmetic the sessions need: Easter, the third Saturday of a
/// month, every second Sunday.
/// </summary>
public static class Calendar
{
    /// <summary>Easter Sunday (Gregorian), by the anonymous Gregorian algorithm.</summary>
    public static DateOnly Easter(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }

    /// <summary>The nth (1-based) given weekday of a month, such as the 3rd Saturday.</summary>
    public static DateOnly NthWeekday(int year, int month, DayOfWeek day, int n)
    {
        var first = new DateOnly(year, month, 1);
        var offset = ((int)day - (int)first.DayOfWeek + 7) % 7;
        return first.AddDays(offset + (n - 1) * 7);
    }

    /// <summary>The first date on or after <paramref name="from"/> that falls on <paramref name="day"/>.</summary>
    public static DateOnly OnOrAfter(DateOnly from, DayOfWeek day) =>
        from.AddDays(((int)day - (int)from.DayOfWeek + 7) % 7);

    /// <summary>Every <paramref name="everyWeeks"/> weeks on one weekday, from start to end inclusive.</summary>
    public static IEnumerable<DateOnly> Every(
        DateOnly start,
        DateOnly end,
        DayOfWeek day,
        int everyWeeks = 1
    )
    {
        for (var date = OnOrAfter(start, day); date <= end; date = date.AddDays(7 * everyWeeks))
            yield return date;
    }

    /// <summary>The nth given weekday of every month from start to end inclusive.</summary>
    public static IEnumerable<DateOnly> MonthlyOn(
        DateOnly start,
        DateOnly end,
        DayOfWeek day,
        int n
    )
    {
        for (
            var month = new DateOnly(start.Year, start.Month, 1);
            month <= end;
            month = month.AddMonths(1)
        )
        {
            var date = NthWeekday(month.Year, month.Month, day, n);
            if (date >= start && date <= end)
                yield return date;
        }
    }

    /// <summary>The rains in southern Ghana: the long one around April to June, the short one in October.</summary>
    public static bool IsRainySeason(DateOnly date) => date.Month is >= 4 and <= 6 or 10;
}
