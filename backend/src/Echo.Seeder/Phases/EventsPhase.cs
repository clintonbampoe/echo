using Echo.Domain.Events;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Creates events with registrations and attendance.
///
/// Event names are unique per congregation (unique index on CongregationId + Name).
/// We append an index to ensure uniqueness when picking from the stem list.
///
/// For each event:
///   - A random subset of members register in advance.
///   - 70% of registrants actually attend.
///
/// The unique index on (EventId, MemberId) for both EventRegistrations and
/// EventAttendances is respected by using rng.Shuffle + Take to pick distinct
/// members per event — same member can't register or attend twice.
///
/// Output written to state: EventIds on each CongregationSeed.
/// </summary>
public sealed class EventsPhase : ISeedPhase
{
    public string Name => "Events";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.MemberIds.Count == 0 || seed.OrganizationIds.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members or orgs, skipping events.",
                    seed.Congregation.Name
                );
                continue;
            }

            var eventCount = seed.IsBig ? rng.Next(40, 400) : rng.Next(5, 100);
            var memberList = seed.MemberIds.ToList();

            // ── Events ────────────────────────────────────────────────────
            var events = Enumerable
                .Range(0, eventCount)
                .Select(i =>
                {
                    var startDate = rng.RecentDate(ctx.Now, 365 * 2);
                    var duration = rng.Next(1, 4);

                    return new Event
                    {
                        Id = Guid.NewGuid(),
                        CongregationId = seed.Id,
                        OrganizationId = rng.Pick(seed.OrganizationIds),
                        OrganizerId = rng.Pick(memberList),
                        // Index appended to guarantee uniqueness within congregation
                        Name = $"{rng.Pick(_eventStems)} {i + 1}",
                        StartDate = startDate,
                        EndDate = startDate.AddDays(duration),
                        StartTime = rng.Chance(0.7) ? new TimeOnly(rng.Next(6, 20), 0) : null,
                        EndTime = rng.Chance(0.7) ? new TimeOnly(rng.Next(11, 22), 0) : null,
                        Location = rng.Chance(0.6) ? rng.Pick(_locations) : null,
                        Capacity = rng.Chance(0.5) ? rng.Next(50, 2000) : null,
                        Description = null,
                        CreatedAt = ctx.Now,
                    };
                })
                .ToList();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                events,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventsAsync,
                null,
                ct
            );

            foreach (var e in events)
                seed.EventIds.Add(e.Id);

            // ── Registrations and attendance ──────────────────────────────
            var registrations = new List<EventRegistration>();
            var attendances = new List<EventAttendance>();

            foreach (var evt in events)
            {
                var regCount = rng.Next(10, Math.Max(11, memberList.Count / 20));
                // Shuffle gives distinct members — no duplicate (EventId, MemberId)
                var chosen = rng.Shuffle(memberList).Take(Math.Min(regCount, memberList.Count));

                foreach (var memberId in chosen)
                {
                    // Registration date is 1–30 days before the event starts
                    var regDate = evt.StartDate.AddDays(-rng.Next(1, 30));

                    registrations.Add(
                        new EventRegistration
                        {
                            Id = Guid.NewGuid(),
                            CongregationId = seed.Id,
                            MemberId = memberId,
                            EventId = evt.Id,
                            RegistrationDate = regDate,
                            CreatedAt = ctx.Now,
                        }
                    );

                    // 70% of registrants actually attended
                    if (rng.Chance(0.7))
                    {
                        attendances.Add(
                            new EventAttendance
                            {
                                Id = Guid.NewGuid(),
                                CongregationId = seed.Id,
                                MemberId = memberId,
                                EventId = evt.Id,
                                CheckInTime = new TimeOnly(rng.Next(6, 20), rng.Next(0, 60)),
                                CreatedAt = ctx.Now,
                            }
                        );
                    }
                }
            }

            await CopyBatch.WriteAsync(
                ctx.Connection,
                registrations,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventRegistrationsAsync,
                null,
                ct
            );

            await CopyBatch.WriteAsync(
                ctx.Connection,
                attendances,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventAttendancesAsync,
                null,
                ct
            );

            ctx.Logger.LogInformation(
                "{Congregation}: {E} events, {R} registrations, {A} attendance.",
                seed.Congregation.Name,
                events.Count,
                registrations.Count,
                attendances.Count
            );
        }
    }

    private static readonly IReadOnlyList<string> _eventStems =
    [
        "Annual Convention",
        "Youth Camp",
        "Leadership Retreat",
        "Harvest Festival",
        "Prayer Summit",
        "Easter Service",
        "Christmas Carol",
        "Revival Night",
        "Missions Conference",
        "Thanksgiving Service",
        "Women's Conference",
        "Men's Retreat",
        "Family Fun Day",
        "Charity Outreach",
    ];

    private static readonly IReadOnlyList<string> _locations =
    [
        "Main Hall",
        "Church Auditorium",
        "Fellowship Hall",
        "Youth Center",
        "Open Grounds",
    ];
}
