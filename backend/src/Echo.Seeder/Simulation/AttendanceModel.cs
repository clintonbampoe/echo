using Echo.Domain.Attendances;

namespace Echo.Seeder.Simulation;

/// <summary>
/// The rules of who goes to what. Pure functions of a person and a date, so
/// they can be tested without a database.
/// </summary>
public static class AttendanceModel
{
    /// <summary>
    /// The chance that someone with this commitment (0 to 1) is at an ordinary
    /// Sunday service. About 0.1 for the barely committed, 0.45 in the middle
    /// and 0.9 for the faithful core.
    /// </summary>
    public static double ServiceChance(double commitment) => 0.04 + 0.9 * Math.Pow(commitment, 1.3);

    /// <summary>The chance of attending a session of this kind, before the date's effects.</summary>
    public static double BaseChance(SessionCategory category, double commitment)
    {
        var service = ServiceChance(commitment);

        return category switch
        {
            SessionCategory.Service => service,
            SessionCategory.Special => Math.Min(0.97, service * 1.15),
            SessionCategory.Study => service * 0.38,
            SessionCategory.Prayer => service * 0.3,
            SessionCategory.Practice => 0.55 + 0.4 * commitment,
            SessionCategory.Meeting => 0.75 + 0.2 * commitment,
            SessionCategory.Outreach => 0.35 + 0.4 * commitment,
            SessionCategory.Class => 0.7,
            _ => 0.0,
        };
    }

    /// <summary>
    /// How the date changes attendance: more people at Easter and Christmas,
    /// fewer in the July and August holidays and in the rains, a few more on
    /// the first Sunday of the month (communion).
    /// </summary>
    public static double Seasonal(DateOnly date, SessionCategory category)
    {
        var factor = 1.0;
        var isService = category is SessionCategory.Service or SessionCategory.Special;

        var easter = Calendar.Easter(date.Year);
        var nearEaster = Math.Abs(date.DayNumber - easter.DayNumber) <= 2;
        var nearChristmas = date.Month == 12 && date.Day >= 24;
        if (nearEaster || nearChristmas)
            factor *= 1.3;

        if (date.Month is 7 or 8)
            factor *= 0.93;

        if (date.Month == 1 && date.Day <= 14)
            factor *= 1.05;

        if (Calendar.IsRainySeason(date))
            factor *= isService ? 0.96 : 0.85;

        if (category == SessionCategory.Service && date.Day <= 7)
            factor *= 1.06;

        return factor;
    }

    /// <summary>
    /// People drift away: in the four months before someone leaves, they come
    /// less and less.
    /// </summary>
    public static double FadeBeforeLeaving(DateOnly date, DateOnly? leftOn)
    {
        if (leftOn is not { } left)
            return 1.0;

        var daysLeft = left.DayNumber - date.DayNumber;
        return daysLeft >= 120 ? 1.0 : Math.Max(0.1, daysLeft / 120.0);
    }

    /// <summary>New members are keen: a bump for their first eight weeks.</summary>
    public static double NewcomerBoost(DateOnly date, DateOnly joinedOn) =>
        date.DayNumber - joinedOn.DayNumber < 56 ? 1.25 : 1.0;
}
