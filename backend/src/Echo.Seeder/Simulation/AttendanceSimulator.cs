using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>
/// Produces every attendance row for one congregation, session by session.
/// <br/><br/>
/// Members attend according to <see cref="AttendanceModel"/>, but not
/// independently: a household tends to go together, so on a given Sunday most
/// of a family is there or most of it is not. Children go when their parents
/// go. In the big congregation each household keeps to one of the two services.
/// Midweek and group sessions only draw from the people they are for: choir
/// practice from the music ministries, the leaders' meeting from the leaders,
/// the membership class from people who joined in the last nine months.
/// <br/><br/>
/// Visitors come on the Sunday they first visited and sometimes a few more
/// times; those who converted keep coming until they become members, and are
/// recorded as members from that day on.
/// </summary>
public sealed class AttendanceSimulator
{
    private static readonly string[] _musicMinistries =
    [
        "Choir",
        "Praise and Worship Team",
        "Instrumentalists",
        "Dance Ministry",
        "Drama Team",
    ];

    private static readonly string[] _outreachMinistries =
    [
        "Evangelism Team",
        "Charity Outreach",
        "Missions Committee",
        "Visitation Team",
        "Welfare Committee",
        "Youth Ministry",
    ];

    private static readonly string[] _leadingBodies = ["Elders' Council", "Finance Committee"];

    private readonly CongregationSeed _seed;
    private readonly Rng _rng;
    private readonly DateTime _now;
    private readonly PersonProfile[] _members;
    private readonly int[] _household;
    private readonly int _households;
    private readonly HashSet<Guid> _music;
    private readonly HashSet<Guid> _outreach;
    private readonly HashSet<Guid> _leaders;
    private readonly Dictionary<DateOnly, List<Session>> _servicesByDate;

    public AttendanceSimulator(CongregationSeed seed, Rng rng, DateTime now)
    {
        _seed = seed;
        _rng = rng;
        _now = now;
        _members = [.. seed.Members];

        var index = new Dictionary<Guid, int>();
        _household = new int[_members.Length];
        for (var i = 0; i < _members.Length; i++)
        {
            if (!index.TryGetValue(_members[i].HouseholdId, out var h))
                index[_members[i].HouseholdId] = h = index.Count;

            _household[i] = h;
        }
        _households = index.Count;

        _music = MembersOf(seed, _musicMinistries);
        _outreach = MembersOf(seed, _outreachMinistries);
        _leaders = [.. seed.Organizations.Select(o => o.LeaderId)];
        _leaders.UnionWith(MembersOf(seed, _leadingBodies));

        _servicesByDate = seed
            .Sessions.Where(s => s.SessionLabel.Category == SessionCategory.Service)
            .GroupBy(s => s.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public IEnumerable<Attendance> Generate()
    {
        foreach (var session in _seed.Sessions.Where(s => s.Status == SessionStatus.Completed))
        {
            foreach (var attendance in MembersAt(session))
                yield return attendance;
        }

        foreach (var attendance in VisitorsAttending())
            yield return attendance;
    }

    // ── Members ───────────────────────────────────────────────────────────

    private IEnumerable<Attendance> MembersAt(Session session)
    {
        var category = session.SessionLabel.Category;
        var date = session.Date;
        var slot = _seed.ServiceSlots.GetValueOrDefault(session.Id);

        // One draw per household, shared by most of its members, plus a mood
        // for the whole session (weather, a long weekend).
        var household = new double[_households];
        for (var h = 0; h < household.Length; h++)
            household[h] = _rng.NextDouble();

        var mood = Math.Clamp(_rng.Gaussian(1.0, 0.05), 0.85, 1.15);
        var togetherness = category is SessionCategory.Service or SessionCategory.Special
            ? 0.75
            : 0.3;
        var seasonal = AttendanceModel.Seasonal(date, category);

        for (var i = 0; i < _members.Length; i++)
        {
            var member = _members[i];
            if (!member.IsMemberOn(date) || !IsFor(member, session, category, slot, _household[i]))
                continue;

            var chance =
                AttendanceModel.BaseChance(category, member.Commitment)
                * seasonal
                * mood
                * AttendanceModel.FadeBeforeLeaving(date, member.LeftOn);

            if (category == SessionCategory.Service)
                chance *= AttendanceModel.NewcomerBoost(date, member.JoinedOn);

            var draw = _rng.Chance(togetherness) ? household[_household[i]] : _rng.NextDouble();
            if (draw >= Math.Min(chance, 0.98))
                continue;

            yield return Record(session, member.Id, PersonKind.Member, visitor: false);
        }
    }

    // Who a session is for.
    private bool IsFor(
        PersonProfile member,
        Session session,
        SessionCategory category,
        int slot,
        int household
    )
    {
        switch (category)
        {
            case SessionCategory.Service:
                // In the big congregation a household keeps to one service.
                return !_seed.IsBig || household % 2 == slot;
            case SessionCategory.Special:
                return true;
            case SessionCategory.Study:
            case SessionCategory.Prayer:
                return member.AgeOn(session.Date) >= 16;
            case SessionCategory.Practice:
                return _music.Contains(member.Id);
            case SessionCategory.Meeting:
                return _leaders.Contains(member.Id);
            case SessionCategory.Outreach:
                return _outreach.Contains(member.Id);
            case SessionCategory.Class:
                var daysSinceJoining = session.Date.DayNumber - member.JoinedOn.DayNumber;
                return daysSinceJoining is >= 0 and <= 270;
            default:
                return false;
        }
    }

    // ── Visitors ──────────────────────────────────────────────────────────

    private IEnumerable<Attendance> VisitorsAttending()
    {
        foreach (var visitor in _seed.Visitors)
        {
            var already = new HashSet<Session>();

            foreach (var session in VisitSessions(visitor))
            {
                if (session is not null && already.Add(session))
                    yield return Record(session, visitor.Id, PersonKind.Visitor, visitor: true);
            }
        }
    }

    // The Sundays a visitor came. Someone who went on to join comes most weeks
    // until the day they join. Everyone else comes back a few times, each time
    // a little less likely than the last, and then stops.
    private IEnumerable<Session?> VisitSessions(VisitorProfile visitor)
    {
        // A visit on a weekday is recorded at the next Sunday service.
        var first = Calendar.OnOrAfter(visitor.FirstVisit, DayOfWeek.Sunday);

        if (visitor.ConvertedOn is { } converted)
        {
            for (var date = first; date < converted; date = date.AddDays(7))
            {
                if (date == first || _rng.Chance(0.7))
                    yield return ServiceOn(date);
            }

            yield break;
        }

        var chanceOfComingBack = 0.55;
        for (var date = first; ; date = date.AddDays(7))
        {
            yield return ServiceOn(date);

            if (!_rng.Chance(chanceOfComingBack))
                yield break;

            chanceOfComingBack *= 0.8;
        }
    }

    // A service held on that day, if one was held and went ahead. Visits before the
    // calendar starts (or on a day with no service) leave no row.
    private Session? ServiceOn(DateOnly date)
    {
        if (!_servicesByDate.TryGetValue(date, out var services))
            return null;

        var session = _rng.Pick(services);
        return session.Status == SessionStatus.Completed ? session : null;
    }

    // ── One row ───────────────────────────────────────────────────────────

    private Attendance Record(Session session, Guid personId, PersonKind kind, bool visitor)
    {
        var start = session.StartsAt ?? new TimeOnly(9, 0);
        var begins = DateTime.SpecifyKind(session.Date.ToDateTime(start), DateTimeKind.Utc);

        // Most members are checked in by a QR scan with an arrival time; the
        // rest are ticked off by hand by the admin.
        var scanned = _rng.Chance(visitor ? 0.3 : 0.65);
        DateTime? checkedInAt = scanned
            ? begins.AddMinutes(Math.Clamp(_rng.Gaussian(2, 12), -30, 60))
            : null;

        var createdAt = checkedInAt ?? begins.AddHours(2);

        return new Attendance
        {
            Id = _rng.NewGuid(),
            CongregationId = _seed.Id,
            SessionId = session.Id,
            Session = session,
            PersonId = personId,
            PersonKindAtTime = kind,
            CheckedInAt = checkedInAt,
            RecordedBy = scanned ? null : _seed.Admin.Id,
            CreatedAt = createdAt > _now ? _now : createdAt,
        };
    }

    private static HashSet<Guid> MembersOf(CongregationSeed seed, string[] organizationNames) =>
        [
            .. seed
                .Organizations.Where(o => organizationNames.Contains(o.Name))
                .SelectMany(o => o.MemberIds),
        ];
}
