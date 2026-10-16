namespace Echo.Seeder.Simulation;

/// <summary>
/// An event a ministry holds, usually every year.
/// </summary>
/// <param name="Organizations">
/// The ministries that could hold it, most likely first. The first one the
/// congregation has holds it; if it has none of them, the event is skipped.
/// </param>
/// <param name="Name">What it is called. The year is added to make each one unique.</param>
/// <param name="Months">
/// The months it is held. One for a yearly event, several for a recurring one.
/// Empty means the anniversary month of the congregation.
/// </param>
/// <param name="Day">The weekday it starts on.</param>
/// <param name="Nth">Which of that weekday in the month: 2 is the second Saturday.</param>
/// <param name="Days">How many days it lasts.</param>
/// <param name="Start">The time it starts on the first day.</param>
/// <param name="End">The time it ends on the last day.</param>
/// <param name="Location">Where it is held.</param>
/// <param name="OpenToAll">
/// True when anyone in the congregation may come. False when it is for the
/// ministry's own members.
/// </param>
/// <param name="Interest">
/// The share of those it is for who come, for someone of average commitment.
/// </param>
/// <param name="CapacityShare">
/// The venue's capacity as a share of the active members, or null when the
/// event has no limit.
/// </param>
/// <param name="Description">One line shown in the event's details.</param>
public sealed record EventTemplate(
    string[] Organizations,
    string Name,
    int[] Months,
    DayOfWeek Day,
    int Nth,
    int Days,
    TimeOnly Start,
    TimeOnly End,
    string Location,
    bool OpenToAll,
    double Interest,
    double? CapacityShare,
    string Description
)
{
    /// <summary>True for the yearly thanksgiving on the congregation's anniversary.</summary>
    public bool IsAnniversary => Months.Length == 0;
}

public static class EventCatalog
{
    public static IReadOnlyList<EventTemplate> All { get; } =
    [
        new(
            ["Youth Ministry"],
            "Youth Camp",
            [8],
            DayOfWeek.Thursday,
            1,
            3,
            new(16, 0),
            new(14, 0),
            "Retreat Centre",
            false,
            0.5,
            0.05,
            "Three days of teaching, worship and games for the youth."
        ),
        new(
            ["Youth Ministry"],
            "Youth Sunday",
            [10],
            DayOfWeek.Sunday,
            2,
            1,
            new(8, 0),
            new(13, 0),
            "Main Sanctuary",
            true,
            0.5,
            null,
            "A service led entirely by the youth."
        ),
        new(
            ["Teens Ministry", "Youth Ministry"],
            "Teens Summit",
            [4],
            DayOfWeek.Saturday,
            2,
            1,
            new(9, 0),
            new(16, 0),
            "Church Auditorium",
            false,
            0.5,
            0.03,
            "A day of talks and workshops for teenagers."
        ),
        new(
            ["Women's Fellowship"],
            "Women's Conference",
            [5],
            DayOfWeek.Friday,
            3,
            2,
            new(18, 0),
            new(16, 0),
            "Church Auditorium",
            false,
            0.55,
            0.1,
            "The yearly conference for the women of the congregation."
        ),
        new(
            ["Women's Fellowship"],
            "Mothers' Day Brunch",
            [5],
            DayOfWeek.Sunday,
            2,
            1,
            new(12, 30),
            new(15, 30),
            "Fellowship Hall",
            false,
            0.6,
            0.04,
            "A brunch to honour the mothers."
        ),
        new(
            ["Men's Fellowship"],
            "Men's Retreat",
            [3],
            DayOfWeek.Friday,
            2,
            2,
            new(18, 0),
            new(15, 0),
            "Retreat Centre",
            false,
            0.5,
            0.05,
            "A weekend away for prayer and fellowship."
        ),
        new(
            ["Men's Fellowship"],
            "Fathers' Day Breakfast",
            [6],
            DayOfWeek.Sunday,
            3,
            1,
            new(7, 30),
            new(10, 30),
            "Fellowship Hall",
            false,
            0.55,
            0.04,
            "A breakfast to honour the fathers."
        ),
        new(
            ["Children's Ministry"],
            "Vacation Bible School",
            [8],
            DayOfWeek.Monday,
            2,
            5,
            new(9, 0),
            new(13, 0),
            "Church Auditorium",
            false,
            0.6,
            0.08,
            "A week of Bible stories, songs and crafts for the children."
        ),
        new(
            ["Children's Ministry"],
            "Children's Christmas Party",
            [12],
            DayOfWeek.Saturday,
            3,
            1,
            new(10, 0),
            new(14, 0),
            "Fellowship Hall",
            false,
            0.7,
            null,
            "Games, food and gifts for the children."
        ),
        new(
            ["Sunday School Teachers", "Children's Ministry"],
            "Teachers' Training",
            [1],
            DayOfWeek.Saturday,
            4,
            1,
            new(9, 0),
            new(15, 0),
            "Fellowship Hall",
            false,
            0.6,
            0.01,
            "Training for the Sunday school teachers."
        ),
        new(
            ["Choir"],
            "Annual Choir Concert",
            [11],
            DayOfWeek.Saturday,
            3,
            1,
            new(17, 0),
            new(20, 30),
            "Church Auditorium",
            true,
            0.22,
            0.2,
            "An evening of music by the choir and invited guests."
        ),
        new(
            ["Dance Ministry", "Drama Team"],
            "Annual Drama Night",
            [7],
            DayOfWeek.Friday,
            4,
            1,
            new(18, 0),
            new(21, 0),
            "Church Auditorium",
            true,
            0.12,
            null,
            "Sketches and dance by the creative ministries."
        ),
        new(
            ["Prayer Team", "Intercessors"],
            "Prayer and Fasting Week",
            [1],
            DayOfWeek.Monday,
            2,
            5,
            new(5, 30),
            new(7, 0),
            "Main Sanctuary",
            true,
            0.12,
            null,
            "A week of early morning prayer to begin the year."
        ),
        new(
            ["Prayer Team", "Intercessors"],
            "All-Night Prayer",
            [3, 6, 9, 12],
            DayOfWeek.Friday,
            4,
            2,
            new(22, 0),
            new(5, 0),
            "Main Sanctuary",
            true,
            0.08,
            null,
            "A quarterly night of prayer."
        ),
        new(
            ["Evangelism Team"],
            "Evangelistic Crusade",
            [6],
            DayOfWeek.Thursday,
            2,
            3,
            new(17, 0),
            new(21, 0),
            "Open Grounds",
            true,
            0.15,
            null,
            "Three nights of open-air preaching and prayer."
        ),
        new(
            ["Evangelism Team"],
            "Evangelism Training",
            [2],
            DayOfWeek.Saturday,
            3,
            1,
            new(9, 0),
            new(15, 0),
            "Fellowship Hall",
            false,
            0.35,
            0.02,
            "Training in sharing the gospel."
        ),
        new(
            ["Missions Committee", "Evangelism Team"],
            "Missions Conference",
            [9],
            DayOfWeek.Friday,
            3,
            2,
            new(17, 0),
            new(15, 0),
            "Church Auditorium",
            true,
            0.2,
            0.15,
            "Reports and appeals from the congregation's missionaries."
        ),
        new(
            ["Welfare Committee", "Charity Outreach"],
            "Widows' Thanksgiving Day",
            [11],
            DayOfWeek.Saturday,
            2,
            1,
            new(10, 0),
            new(14, 0),
            "Fellowship Hall",
            false,
            0.3,
            0.03,
            "A lunch and gifts for the widows of the congregation."
        ),
        new(
            ["Charity Outreach", "Welfare Committee", "Evangelism Team"],
            "Christmas Charity Drive",
            [12],
            DayOfWeek.Saturday,
            2,
            1,
            new(8, 0),
            new(14, 0),
            "Community Centre",
            true,
            0.15,
            null,
            "Food and clothing for needy families nearby."
        ),
        new(
            ["Marriage Ministry"],
            "Marriage Seminar",
            [2],
            DayOfWeek.Saturday,
            2,
            1,
            new(9, 0),
            new(15, 0),
            "Church Auditorium",
            false,
            0.5,
            0.05,
            "Teaching and counselling for married couples."
        ),
        new(
            ["Marriage Ministry"],
            "Couples' Dinner",
            [2],
            DayOfWeek.Saturday,
            3,
            1,
            new(18, 0),
            new(21, 30),
            "Fellowship Hall",
            false,
            0.45,
            0.02,
            "A Valentine dinner for couples."
        ),
        new(
            ["Singles Fellowship", "Young Adults Fellowship"],
            "Singles Retreat",
            [10],
            DayOfWeek.Friday,
            2,
            2,
            new(18, 0),
            new(15, 0),
            "Retreat Centre",
            false,
            0.5,
            0.02,
            "A weekend for unmarried adults."
        ),
        new(
            ["Young Adults Fellowship", "Youth Ministry"],
            "Career and Business Fair",
            [7],
            DayOfWeek.Saturday,
            2,
            1,
            new(10, 0),
            new(16, 0),
            "Church Auditorium",
            true,
            0.1,
            0.05,
            "Talks and networking for young professionals."
        ),
        new(
            ["Elders' Council", "Prayer Team"],
            "Leadership Retreat",
            [1],
            DayOfWeek.Friday,
            3,
            2,
            new(17, 0),
            new(14, 0),
            "Retreat Centre",
            false,
            0.7,
            0.01,
            "Planning and prayer for the year ahead."
        ),
        new(
            ["Elders' Council", "Prayer Team"],
            "Anniversary Thanksgiving Service",
            [],
            DayOfWeek.Sunday,
            2,
            1,
            new(8, 0),
            new(13, 0),
            "Main Sanctuary",
            true,
            0.55,
            null,
            "Thanksgiving for another year of the congregation."
        ),
        new(
            ["Elders' Council", "Prayer Team"],
            "Harvest Dinner",
            [11],
            DayOfWeek.Saturday,
            1,
            1,
            new(18, 0),
            new(21, 30),
            "Church Auditorium",
            true,
            0.25,
            0.2,
            "A dinner after the harvest thanksgiving appeal."
        ),
        new(
            ["Seniors' Fellowship"],
            "Seniors' Day Out",
            [6],
            DayOfWeek.Saturday,
            1,
            1,
            new(10, 0),
            new(16, 0),
            "Fellowship Hall",
            false,
            0.5,
            null,
            "A day of fellowship and games for older members."
        ),
        new(
            ["Health Ministry"],
            "Health Walk and Screening",
            [4],
            DayOfWeek.Saturday,
            1,
            1,
            new(6, 0),
            new(11, 0),
            "Open Grounds",
            true,
            0.12,
            null,
            "A walk followed by free health screening."
        ),
        new(
            ["Media Team", "Technical Team"],
            "Media Training Workshop",
            [3],
            DayOfWeek.Saturday,
            4,
            1,
            new(9, 0),
            new(15, 0),
            "Fellowship Hall",
            false,
            0.6,
            0.005,
            "Training on sound, projection and streaming."
        ),
    ];
}
