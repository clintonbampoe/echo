using System.Globalization;
using Echo.Domain.Events;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>An event and the template it came from.</summary>
public sealed record PlannedEvent(Event Event, EventTemplate Template);

/// <summary>Who registered for the events and who came.</summary>
public sealed record EventParticipation(
    List<EventRegistration> Registrations,
    List<EventAttendance> Attendances
);

/// <summary>
/// Plans a congregation's events and who takes part.
/// <br/><br/>
/// Events come from <see cref="EventCatalog"/>. Each is held by a ministry the
/// congregation actually has, led by that ministry's leader, on the same slot
/// every year (the second Saturday of May, say), though now and then a year is
/// skipped. Nothing is held before its ministry began.
/// <br/><br/>
/// Who takes part follows interest and commitment: an event for a ministry draws
/// that ministry's members, an open event draws from everyone, and a household
/// tends to come together. A venue with a limit fills up with the keenest first.
/// Most registrants come, a few extra walk in, and events that have not
/// happened yet have registrations only.
/// </summary>
public sealed class EventPlanner(
    CongregationSeed seed,
    Rng rng,
    DateOnly today,
    DateTime now,
    int historyMonths
)
{
    private const int _upcomingDays = 120;

    private readonly DateOnly _from = Later(
        seed.Founded,
        new DateOnly(today.Year, today.Month, 1).AddMonths(-(historyMonths - 1))
    );

    // ── Events ────────────────────────────────────────────────────────────

    public List<PlannedEvent> Plan()
    {
        var organizations = seed.Organizations.ToDictionary(o => o.Name);
        var activeMembers = seed.Members.Count(m => m.LeftOn is null);
        var to = today.AddDays(_upcomingDays);
        var planned = new List<PlannedEvent>();

        foreach (var template in EventCatalog.All)
        {
            var organization = template
                .Organizations.Select(name => organizations.GetValueOrDefault(name))
                .FirstOrDefault(o => o is not null);

            if (organization is null)
                continue;

            int[] months = template.IsAnniversary ? [seed.Founded.Month] : template.Months;

            for (var year = _from.Year; year <= to.Year; year++)
            {
                foreach (var month in months)
                {
                    var date = Calendar.NthWeekday(year, month, template.Day, template.Nth);

                    if (date < _from || date > to || date < organization.Founded.AddDays(60))
                        continue;

                    // A congregation cannot celebrate an anniversary in the year it began.
                    if (template.IsAnniversary && year <= seed.Founded.Year)
                        continue;

                    // Not every event happens every year.
                    if (rng.Chance(0.1))
                        continue;

                    planned.Add(
                        new PlannedEvent(
                            Build(template, organization, date, year, month, activeMembers),
                            template
                        )
                    );
                }
            }
        }

        return [.. planned.OrderBy(p => p.Event.StartDate)];
    }

    private Event Build(
        EventTemplate template,
        OrganizationProfile organization,
        DateOnly date,
        int year,
        int month,
        int activeMembers
    )
    {
        // Some venues have a limit; the limit is about a share of the congregation.
        int? capacity =
            template.CapacityShare is { } share && rng.Chance(0.7)
                ? Math.Max(30, (int)Money.RoundNicely(activeMembers * share))
                : null;

        return new Event
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            OrganizationId = organization.Id,
            OrganizerId = organization.LeaderId,
            Name = NameFor(template, year, month),
            StartDate = date,
            EndDate = date.AddDays(template.Days - 1),
            StartTime = template.Start,
            EndTime = template.End,
            Location = template.Location,
            Capacity = capacity,
            Description = template.Description,
            CreatedAt = Stamps.On(
                Later(organization.Founded, date.AddDays(-rng.Next(30, 120))),
                rng,
                now
            ),
        };
    }

    private string NameFor(EventTemplate template, int year, int month)
    {
        if (template.IsAnniversary)
            return $"{Ordinal(year - seed.Founded.Year)} {template.Name}";

        if (template.Months.Length > 1)
        {
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            return $"{template.Name} {monthName} {year}";
        }

        return $"{template.Name} {year}";
    }

    // ── Who takes part ────────────────────────────────────────────────────

    public EventParticipation Participate(IReadOnlyList<PlannedEvent> planned)
    {
        var registrations = new List<EventRegistration>();
        var attendances = new List<EventAttendance>();
        var ministries = seed.Organizations.ToDictionary(o => o.Id, o => o.MemberIds.ToHashSet());

        foreach (var item in planned)
            TakePart(item, ministries[item.Event.OrganizationId], registrations, attendances);

        return new EventParticipation(registrations, attendances);
    }

    private void TakePart(
        PlannedEvent item,
        HashSet<Guid> ministry,
        List<EventRegistration> registrations,
        List<EventAttendance> attendances
    )
    {
        var evt = item.Event;
        var template = item.Template;
        var held = evt.EndDate < today;

        // People who might come, keenest first. A household shares most of its draw.
        var household = new Dictionary<Guid, double>();
        var candidates = new List<(PersonProfile Member, double Draw, double Chance)>();

        foreach (var member in seed.Members)
        {
            var inMinistry = ministry.Contains(member.Id);
            if (!member.IsMemberOn(evt.StartDate) || (!template.OpenToAll && !inMinistry))
                continue;

            var chance = ChanceOfComing(template, member, inMinistry, evt.StartDate);

            if (!household.TryGetValue(member.HouseholdId, out var shared))
                household[member.HouseholdId] = shared = rng.NextDouble();

            var draw = rng.Chance(0.6) ? shared : rng.NextDouble();

            // Those just past their chance may still walk in on the day.
            if (draw < chance * 1.3)
                candidates.Add((member, draw, chance));
        }

        var registered = candidates.Where(c => c.Draw < c.Chance).OrderBy(c => c.Draw).ToList();
        if (evt.Capacity is { } capacity && registered.Count > capacity)
            registered = registered.Take(capacity).ToList();

        var registeredIds = registered.Select(c => c.Member.Id).ToHashSet();
        var start = template.Start;
        var attending = 0;

        foreach (var (member, _, _) in registered)
        {
            registrations.Add(Register(evt, member));

            var comes = held && rng.Chance(0.65 + 0.3 * member.Commitment);
            if (!comes)
                continue;

            attendances.Add(CheckIn(evt, member, start));
            attending++;
        }

        if (!held)
            return;

        // Walk-ins: interested enough to come, but did not register. Not when the venue is full.
        foreach (var (member, _, _) in candidates.Where(c => !registeredIds.Contains(c.Member.Id)))
        {
            if (evt.Capacity is { } limit && attending >= limit)
                break;

            attendances.Add(CheckIn(evt, member, start));
            attending++;
        }
    }

    // Committed people come more. Ministry members come more to open events too,
    // and young children rarely come to an event that is not for them.
    private static double ChanceOfComing(
        EventTemplate template,
        PersonProfile member,
        bool inMinistry,
        DateOnly date
    )
    {
        var commitment = member.Commitment;
        var chance = template.Interest * (0.25 + 1.5 * commitment * commitment) / 0.7;

        if (template.OpenToAll)
        {
            if (inMinistry)
                chance *= 1.5;

            if (member.AgeOn(date) < 13)
                chance *= 0.5;
        }

        return Math.Clamp(chance, 0.0, 0.95);
    }

    private EventRegistration Register(Event evt, PersonProfile member)
    {
        // People sign up a few days to a few weeks ahead, never before they joined
        // and never in the future.
        var date = evt.StartDate.AddDays(-rng.Next(3, 46));
        date = Later(date, member.JoinedOn);
        date = Earlier(Earlier(date, today), evt.StartDate);

        return new EventRegistration
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            MemberId = member.Id,
            EventId = evt.Id,
            RegistrationDate = date,
            CreatedAt = Stamps.On(date, rng, now),
        };
    }

    private EventAttendance CheckIn(Event evt, PersonProfile member, TimeOnly start)
    {
        // Most arrive around the start; a few are early or late.
        var minutes =
            start.Hour * 60 + start.Minute + (int)Math.Clamp(rng.Gaussian(5, 15), -20, 90);
        minutes = Math.Clamp(minutes, 0, 24 * 60 - 1);

        return new EventAttendance
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            MemberId = member.Id,
            EventId = evt.Id,
            CheckInTime = new TimeOnly(minutes / 60, minutes % 60),
            CreatedAt = Stamps.On(evt.StartDate, rng, now),
        };
    }

    private static string Ordinal(int n)
    {
        if (n % 100 is 11 or 12 or 13)
            return $"{n}th";

        return (n % 10) switch
        {
            1 => $"{n}st",
            2 => $"{n}nd",
            3 => $"{n}rd",
            _ => $"{n}th",
        };
    }

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;

    private static DateOnly Earlier(DateOnly a, DateOnly b) => a < b ? a : b;
}
