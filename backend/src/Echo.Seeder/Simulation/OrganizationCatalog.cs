using Echo.Domain.Members;
using Echo.Domain.Organizations;

namespace Echo.Seeder.Simulation;

/// <summary>An organization as the simulation sees it: who is in it and who leads it.</summary>
public sealed record OrganizationProfile(
    Guid Id,
    string Name,
    DateOnly Founded,
    Guid LeaderId,
    IReadOnlyList<Guid> MemberIds
);

/// <summary>Who an organization is meant for.</summary>
public enum Audience
{
    Everyone,
    Children,
    Teens,
    Youth,
    Women,
    Men,
    Adults,
}

/// <summary>
/// One kind of ministry.
/// </summary>
/// <param name="Name">What it is called.</param>
/// <param name="Audience">Who can join.</param>
/// <param name="Reach">
/// The share of eligible people who belong, 0 to 1. A youth ministry has most
/// of the youth; the ushering team has a few percent of the congregation.
/// </param>
/// <param name="Core">True for ministries every congregation has from early on.</param>
/// <param name="Description">One line shown in the organization's details.</param>
public sealed record OrganizationKind(
    string Name,
    Audience Audience,
    double Reach,
    bool Core,
    string Description
);

public static class OrganizationCatalog
{
    /// <summary>What a ministry is for, so the organizations can be filtered by kind.</summary>
    public static OrganizationType TypeOf(string name) =>
        name switch
        {
            "Choir" or "Praise and Worship Team" or "Instrumentalists" or "Dance Ministry" =>
                OrganizationType.Music,
            "Ushering Team"
            or "Protocol Team"
            or "Hospitality Team"
            or "Security Team"
            or "Transport Team"
            or "Media Team"
            or "Technical Team"
            or "Beautification Team"
            or "Library and Bookstall" => OrganizationType.Service,
            "Women's Fellowship"
            or "Men's Fellowship"
            or "Singles Fellowship"
            or "Young Adults Fellowship"
            or "Seniors' Fellowship"
            or "Youth Ministry"
            or "Teens Ministry"
            or "Marriage Ministry" => OrganizationType.Fellowship,
            "Children's Ministry"
            or "Sunday School Teachers"
            or "Bible Study Group"
            or "Disciples Class" => OrganizationType.Teaching,
            "Evangelism Team"
            or "Visitation Team"
            or "Charity Outreach"
            or "Missions Committee"
            or "Welfare Committee"
            or "Health Ministry"
            or "Counseling Team" => OrganizationType.Outreach,
            "Prayer Team" or "Intercessors" => OrganizationType.Prayer,
            "Finance Committee" or "Elders' Council" => OrganizationType.Leadership,
            _ => OrganizationType.Other,
        };

    /// <summary>The day each kind of group usually meets.</summary>
    public static DayOfWeek? MeetingDayOf(OrganizationType type) =>
        type switch
        {
            OrganizationType.Music => DayOfWeek.Thursday,
            OrganizationType.Service => DayOfWeek.Sunday,
            OrganizationType.Fellowship => DayOfWeek.Saturday,
            OrganizationType.Teaching => DayOfWeek.Wednesday,
            OrganizationType.Outreach => DayOfWeek.Saturday,
            OrganizationType.Prayer => DayOfWeek.Friday,
            OrganizationType.Leadership => DayOfWeek.Tuesday,
            _ => null,
        };

    /// <summary>True when a person of this age and gender fits the audience.</summary>
    public static bool Fits(Audience audience, int age, Gender gender) =>
        audience switch
        {
            Audience.Children => age is >= 3 and <= 12,
            Audience.Teens => age is >= 13 and <= 19,
            Audience.Youth => age is >= 15 and <= 35,
            Audience.Women => gender == Gender.Female && age >= 18,
            Audience.Men => gender == Gender.Male && age >= 18,
            Audience.Adults => age >= 18,
            _ => age >= 12,
        };

    // Ordered by how likely a congregation is to have them: a small one takes
    // the first few, the big one takes nearly all.
    public static IReadOnlyList<OrganizationKind> All { get; } =
    [
        new("Choir", Audience.Everyone, 0.06, true, "Leads the congregation in song at services."),
        new("Ushering Team", Audience.Adults, 0.03, true, "Welcomes and seats people at services."),
        new(
            "Children's Ministry",
            Audience.Children,
            0.75,
            true,
            "Sunday school and activities for children."
        ),
        new(
            "Youth Ministry",
            Audience.Youth,
            0.45,
            true,
            "Fellowship, teaching and events for young people."
        ),
        new(
            "Women's Fellowship",
            Audience.Women,
            0.35,
            true,
            "Fellowship, prayer and support for women."
        ),
        new(
            "Men's Fellowship",
            Audience.Men,
            0.28,
            true,
            "Fellowship, prayer and mentoring for men."
        ),
        new(
            "Prayer Team",
            Audience.Adults,
            0.06,
            true,
            "Intercedes for the congregation and its needs."
        ),
        new(
            "Evangelism Team",
            Audience.Adults,
            0.04,
            false,
            "Shares the gospel through outreach and visits."
        ),
        new(
            "Welfare Committee",
            Audience.Adults,
            0.02,
            false,
            "Supports members in need and those who are bereaved."
        ),
        new(
            "Media Team",
            Audience.Youth,
            0.03,
            false,
            "Handles sound, projection and live streaming."
        ),
        new(
            "Teens Ministry",
            Audience.Teens,
            0.55,
            false,
            "Teaching and activities for teenagers."
        ),
        new(
            "Praise and Worship Team",
            Audience.Everyone,
            0.03,
            false,
            "Leads the praise section of services."
        ),
        new(
            "Instrumentalists",
            Audience.Everyone,
            0.015,
            false,
            "Plays the instruments for services and rehearsals."
        ),
        new(
            "Drama Team",
            Audience.Youth,
            0.03,
            false,
            "Performs sketches and plays at services and events."
        ),
        new("Protocol Team", Audience.Adults, 0.015, false, "Looks after guests and ministers."),
        new(
            "Finance Committee",
            Audience.Adults,
            0.006,
            false,
            "Oversees the congregation's money and reporting."
        ),
        new(
            "Singles Fellowship",
            Audience.Youth,
            0.12,
            false,
            "Fellowship and teaching for unmarried adults."
        ),
        new(
            "Marriage Ministry",
            Audience.Adults,
            0.08,
            false,
            "Teaching and support for married couples."
        ),
        new(
            "Visitation Team",
            Audience.Adults,
            0.02,
            false,
            "Visits the sick and members who have stopped coming."
        ),
        new(
            "Transport Team",
            Audience.Adults,
            0.01,
            false,
            "Runs church transport and organizes travel."
        ),
        new(
            "Hospitality Team",
            Audience.Adults,
            0.02,
            false,
            "Prepares food and refreshments for events."
        ),
        new("Security Team", Audience.Men, 0.01, false, "Looks after the premises and car park."),
        new(
            "Dance Ministry",
            Audience.Youth,
            0.025,
            false,
            "Leads expressive worship through dance."
        ),
        new(
            "Counseling Team",
            Audience.Adults,
            0.01,
            false,
            "Offers pastoral counseling to members."
        ),
        new(
            "Missions Committee",
            Audience.Adults,
            0.015,
            false,
            "Plans and supports mission work."
        ),
        new(
            "Sunday School Teachers",
            Audience.Adults,
            0.02,
            false,
            "Teach the children's Sunday school classes."
        ),
        new(
            "Intercessors",
            Audience.Adults,
            0.02,
            false,
            "Hold prayer watches for the congregation."
        ),
        new(
            "Beautification Team",
            Audience.Adults,
            0.01,
            false,
            "Cleans and decorates the sanctuary."
        ),
        new(
            "Elders' Council",
            Audience.Adults,
            0.004,
            false,
            "Advises the pastor and oversees the congregation."
        ),
        new(
            "Technical Team",
            Audience.Youth,
            0.015,
            false,
            "Looks after the sound system and equipment."
        ),
        new(
            "Young Adults Fellowship",
            Audience.Youth,
            0.2,
            false,
            "Fellowship for young professionals."
        ),
        new(
            "Seniors' Fellowship",
            Audience.Adults,
            0.05,
            false,
            "Fellowship and care for older members."
        ),
        new(
            "Bible Study Group",
            Audience.Adults,
            0.08,
            false,
            "Weekly home group for studying the Bible."
        ),
        new(
            "Disciples Class",
            Audience.Adults,
            0.03,
            false,
            "Trains new believers in the basics of faith."
        ),
        new(
            "Health Ministry",
            Audience.Adults,
            0.01,
            false,
            "Organizes health screenings and talks."
        ),
        new(
            "Charity Outreach",
            Audience.Adults,
            0.015,
            false,
            "Distributes food and clothing to the needy."
        ),
        new(
            "Library and Bookstall",
            Audience.Adults,
            0.004,
            false,
            "Runs the church bookstall and library."
        ),
    ];
}
