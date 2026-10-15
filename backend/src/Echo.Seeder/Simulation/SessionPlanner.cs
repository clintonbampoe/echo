using Echo.Domain.Attendances;
using Echo.Seeder.Core;

namespace Echo.Seeder.Simulation;

/// <summary>A session and, for Sunday services, which of the day's services it is (0 or 1).</summary>
public sealed record PlannedSession(Session Session, int Slot);

/// <summary>
/// Lays out a congregation's calendar the way a real one runs: Sunday services,
/// midweek study, early prayer, choir practice, a monthly leaders' meeting,
/// outreach, a membership class, and the days of the church year (Good Friday,
/// Easter, Christmas, Watchnight, harvest, the annual convention).
/// <br/><br/>
/// Sessions in the past are completed (a few were cancelled, more often in the
/// rainy season); sessions from today on are scheduled.
/// </summary>
public sealed class SessionPlanner(
    Rng rng,
    IReadOnlyDictionary<SessionCategory, SessionLabel> labels,
    Guid congregationId,
    Guid createdBy,
    DateOnly founded,
    DateOnly today,
    DateTime now,
    int weeksBack,
    int weeksForward,
    bool twoServices
)
{
    private static readonly string[] _slotNames = ["First Service", "Second Service"];

    private readonly DateOnly _start = Later(founded, today.AddDays(-7 * weeksBack));
    private readonly DateOnly _end = today.AddDays(7 * weeksForward);
    private readonly TimeOnly _singleServiceStart = new(
        rng.Pick<int>([8, 8, 9]),
        rng.Pick<int>([0, 30])
    );
    private readonly List<PlannedSession> _planned = [];

    private sealed record Occasion(string Name, string Topic);

    public List<PlannedSession> Build()
    {
        var occasions = SundayOccasions();

        AddServices(occasions);

        foreach (var template in SessionCatalog.Weekly)
            AddRecurring(template, occasions);

        AddYearlyDays();

        return
        [
            .. _planned
                .OrderBy(p => p.Session.Date)
                .ThenBy(p => p.Session.StartsAt)
                .ThenBy(p => p.Slot),
        ];
    }

    // ── Sundays ───────────────────────────────────────────────────────────

    private void AddServices(Dictionary<DateOnly, Occasion> occasions)
    {
        var ordinary = 0;

        foreach (var date in Calendar.Every(_start, _end, DayOfWeek.Sunday))
        {
            var occasion = occasions.GetValueOrDefault(date);
            var topic =
                occasion?.Topic
                ?? SessionCatalog.SundayTopics[ordinary++ % SessionCatalog.SundayTopics.Count];

            var slots = twoServices ? 2 : 1;
            for (var slot = 0; slot < slots; slot++)
            {
                var start = twoServices
                    ? new TimeOnly(slot == 0 ? 7 : 9, slot == 0 ? 0 : 30)
                    : _singleServiceStart;
                var name = ServiceName(occasion, slot);

                Add(
                    SessionCategory.Service,
                    date,
                    name,
                    topic,
                    start,
                    TimeSpan.FromMinutes(twoServices ? 120 : 150),
                    cancelChance: occasion is null ? 0.01 : 0.0,
                    slot
                );
            }
        }
    }

    private string ServiceName(Occasion? occasion, int slot)
    {
        if (!twoServices)
            return occasion?.Name ?? "Sunday Service";

        return occasion is null ? _slotNames[slot] : $"{occasion.Name} ({_slotNames[slot]})";
    }

    // The Sundays that are something more than an ordinary Sunday.
    private Dictionary<DateOnly, Occasion> SundayOccasions()
    {
        var occasions = new Dictionary<DateOnly, Occasion>();

        for (var year = _start.Year; year <= _end.Year; year++)
        {
            occasions[Calendar.Easter(year)] = new Occasion("Easter Sunday Service", "He is risen");
            occasions[Calendar.NthWeekday(year, 11, DayOfWeek.Sunday, 1)] = new Occasion(
                "Harvest Thanksgiving Service",
                "Giving thanks for the year"
            );

            var christmas = new DateOnly(year, 12, 25);
            if (christmas.DayOfWeek == DayOfWeek.Sunday)
                occasions[christmas] = new Occasion("Christmas Day Service", "Born to save");
        }

        return occasions;
    }

    // ── Recurring sessions ────────────────────────────────────────────────

    private void AddRecurring(SessionTemplate template, Dictionary<DateOnly, Occasion> occasions)
    {
        var dates = template.IsMonthly
            ? Calendar.MonthlyOn(_start, _end, template.Day, template.NthOfMonth)
            : Calendar.Every(_start, _end, template.Day, template.EveryWeeks);

        var index = 0;
        foreach (var date in dates)
        {
            // Nothing else is held on Good Friday, Christmas Day or New Year's Eve.
            if (IsHoliday(date))
                continue;

            // The membership class follows the service, so it needs a Sunday that has one.
            if (template.Category == SessionCategory.Class && occasions.ContainsKey(date))
                continue;

            var rainy = Calendar.IsRainySeason(date) ? 1.8 : 1.0;

            Add(
                template.Category,
                date,
                template.Name,
                TopicFor(template, index++),
                template.Start,
                template.Length,
                template.CancelChance * rainy,
                slot: 0
            );
        }
    }

    private static string? TopicFor(SessionTemplate template, int index) =>
        template.Category switch
        {
            SessionCategory.Study => SessionCatalog.StudyTopic(index),
            SessionCategory.Class => SessionCatalog.ClassLessons[
                index % SessionCatalog.ClassLessons.Count
            ],
            SessionCategory.Outreach => SessionCatalog.OutreachPlaces[
                index % SessionCatalog.OutreachPlaces.Count
            ],
            _ => null,
        };

    // ── Days of the church year ───────────────────────────────────────────

    private void AddYearlyDays()
    {
        for (var year = _start.Year; year <= _end.Year; year++)
        {
            AddSpecial(
                Calendar.Easter(year).AddDays(-2),
                "Good Friday Service",
                "It is finished",
                new TimeOnly(10, 0),
                TimeSpan.FromHours(3)
            );

            var christmas = new DateOnly(year, 12, 25);
            if (christmas.DayOfWeek != DayOfWeek.Sunday)
                AddSpecial(
                    christmas,
                    "Christmas Day Service",
                    "Born to save",
                    new TimeOnly(9, 0),
                    TimeSpan.FromHours(2)
                );

            AddSpecial(
                new DateOnly(year, 12, 31),
                "Watchnight Service",
                "Crossing over in prayer",
                new TimeOnly(22, 0),
                TimeSpan.FromHours(2)
            );
            AddSpecial(
                Calendar.NthWeekday(year, 8, DayOfWeek.Saturday, 3),
                "Annual Convention",
                "Ablaze for Christ",
                new TimeOnly(9, 0),
                TimeSpan.FromHours(6)
            );
        }
    }

    private void AddSpecial(
        DateOnly date,
        string name,
        string topic,
        TimeOnly start,
        TimeSpan length
    )
    {
        if (date < _start || date > _end)
            return;

        Add(SessionCategory.Special, date, name, topic, start, length, cancelChance: 0.0, slot: 0);
    }

    private static bool IsHoliday(DateOnly date) =>
        date == Calendar.Easter(date.Year).AddDays(-2)
        || (date.Month == 12 && date.Day is 25 or 31);

    // ── One session ───────────────────────────────────────────────────────

    private void Add(
        SessionCategory category,
        DateOnly date,
        string name,
        string? topic,
        TimeOnly start,
        TimeSpan length,
        double cancelChance,
        int slot
    )
    {
        var label = labels[category];
        var end = start.Add(length);

        var status = SessionStatus.Scheduled;
        if (date < today)
            status = rng.Chance(cancelChance) ? SessionStatus.Cancelled : SessionStatus.Completed;

        var session = new Session
        {
            Id = rng.NewGuid(),
            CongregationId = congregationId,
            SessionLabelId = label.Id,
            SessionLabel = label,
            Date = date,
            Name = name,
            Topic = topic,
            Notes = null,
            StartsAt = start,
            // A session that runs past midnight is shown ending at the last minute of the day.
            EndsAt = end < start ? new TimeOnly(23, 59) : end,
            Status = status,
            CreatedBy = createdBy,
            CreatedAt = Stamps.On(date.AddDays(-rng.Next(2, 10)), rng, now),
        };

        _planned.Add(new PlannedSession(session, slot));
    }

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;
}
