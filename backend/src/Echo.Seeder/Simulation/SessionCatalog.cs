using Echo.Domain.Attendances;

namespace Echo.Seeder.Simulation;

/// <summary>A recurring session: what, when, how long, how often.</summary>
public sealed record SessionTemplate(
    SessionCategory Category,
    string Name,
    DayOfWeek Day,
    TimeOnly Start,
    TimeSpan Length,
    int EveryWeeks,
    int NthOfMonth,
    double CancelChance
)
{
    /// <summary>True when it happens on the nth weekday of each month instead of every few weeks.</summary>
    public bool IsMonthly => NthOfMonth > 0;
}

/// <summary>
/// The week of a typical congregation, and the topics its teaching cycles through.
/// </summary>
public static class SessionCatalog
{
    public static IReadOnlyList<SessionTemplate> Weekly { get; } =
    [
        new(
            SessionCategory.Study,
            "Midweek Bible Study",
            DayOfWeek.Wednesday,
            new TimeOnly(18, 0),
            TimeSpan.FromMinutes(90),
            1,
            0,
            0.04
        ),
        new(
            SessionCategory.Prayer,
            "Morning Prayer",
            DayOfWeek.Tuesday,
            new TimeOnly(5, 30),
            TimeSpan.FromHours(1),
            1,
            0,
            0.05
        ),
        new(
            SessionCategory.Practice,
            "Choir Rehearsal",
            DayOfWeek.Thursday,
            new TimeOnly(17, 30),
            TimeSpan.FromHours(2),
            1,
            0,
            0.05
        ),
        new(
            SessionCategory.Class,
            "Membership Class",
            DayOfWeek.Sunday,
            new TimeOnly(12, 30),
            TimeSpan.FromMinutes(90),
            2,
            0,
            0.05
        ),
        new(
            SessionCategory.Meeting,
            "Leaders' Meeting",
            DayOfWeek.Saturday,
            new TimeOnly(10, 0),
            TimeSpan.FromHours(2),
            0,
            2,
            0.05
        ),
        new(
            SessionCategory.Outreach,
            "Community Outreach",
            DayOfWeek.Saturday,
            new TimeOnly(8, 0),
            TimeSpan.FromHours(4),
            0,
            3,
            0.08
        ),
    ];

    public static IReadOnlyList<string> SundayTopics { get; } =
    [
        "Faith that does not give way",
        "Walking in the light",
        "The God who provides",
        "Grace that is enough",
        "A heart of thanksgiving",
        "Being a good neighbour",
        "Rest for the weary",
        "Hope in hard seasons",
        "Prayer that persists",
        "Forgiving as we are forgiven",
        "Wisdom for daily work",
        "Serving without applause",
        "The power of a small start",
        "Standing firm under pressure",
        "Joy that does not depend on circumstance",
        "Generosity as a way of life",
        "Raising children in faith",
        "Love in the home",
        "Called to be different",
        "Trusting when we cannot see",
    ];

    public static IReadOnlyList<(string Book, int Chapters)> StudyBooks { get; } =
    [
        ("Romans", 16),
        ("James", 5),
        ("Ephesians", 6),
        ("Acts", 28),
        ("Philippians", 4),
        ("1 Peter", 5),
        ("Proverbs", 31),
        ("Galatians", 6),
    ];

    public static IReadOnlyList<string> ClassLessons { get; } =
    ["Salvation", "Baptism", "Membership", "Giving", "The Holy Spirit", "Prayer", "Serving"];

    public static IReadOnlyList<string> OutreachPlaces { get; } =
    [
        "Market outreach",
        "Hospital visit",
        "Orphanage visit",
        "Street evangelism",
        "Neighbourhood clean-up",
        "Prison ministry visit",
        "Door-to-door visits",
    ];

    /// <summary>The Bible study topic for the nth week: a chapter of a book, book after book.</summary>
    public static string StudyTopic(int week)
    {
        var total = StudyBooks.Sum(b => b.Chapters);
        var position = week % total;

        foreach (var (book, chapters) in StudyBooks)
        {
            if (position < chapters)
                return $"{book} {position + 1}";

            position -= chapters;
        }

        return StudyBooks[0].Book;
    }
}
