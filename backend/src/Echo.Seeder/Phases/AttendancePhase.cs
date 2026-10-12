using Echo.Domain.Attendances;
using Echo.Domain.Members;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates attendance records for both members and visitors.
/// <br/><br/>
/// Every attendance belongs to a session. For each session, a random subset
/// of eligible members and a smaller subset of eligible visitors are marked
/// present.
/// <br/><br/>
/// A converted visitor is never eligible as both a member and a visitor for
/// the same session. Eligibility depends on the session date relative to the
/// visitor's conversion instant:
/// <br/><br/>
/// - Session before conversion → eligible as Visitor.<br/>
/// - Session on or after conversion → eligible as Member.
/// <br/><br/>
/// Sessions are walked in ascending date order so the conversion boundary
/// moves forward exactly once per conversion, never re-scanned. The eligible
/// member and visitor pools are disjoint by construction, which is what keeps
/// the (SessionId, PersonId) unique index happy.
/// </summary>
public sealed class AttendancePhase : ISeedPhase
{
    public string Name => "Attendance";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.Sessions.Count == 0 || seed.MemberIds.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no sessions or members, skipping attendance.",
                    seed.Congregation.Name
                );
                continue;
            }

            var source = GenerateAttendance(ctx, rng, seed);

            await CopyBatch.WriteAsync(
                ctx.Connection,
                source,
                ctx.Options.BatchSize,
                AttendanceCopy.WriteAttendancesAsync,
                n =>
                    ctx.Logger.LogInformation(
                        "  {Congregation} attendance: {N}",
                        seed.Congregation.Name,
                        n
                    ),
                ct
            );
        }
    }

    private static IEnumerable<Attendance> GenerateAttendance(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed
    )
    {
        // Split the population once per congregation. Converted visitors are
        // excluded from the "always" pools because their eligibility flips
        // per session.
        var convertedIds = new HashSet<Guid>(seed.ConvertedVisitorDates.Keys);
        var alwaysMembers = seed.MemberIds.Where(id => !convertedIds.Contains(id)).ToList();
        var alwaysVisitors = seed.VisitorIds.Where(id => !convertedIds.Contains(id)).ToList();

        // Conversions sorted by instant. Walked in lockstep with sessions.
        var conversions = seed
            .ConvertedVisitorDates.OrderBy(x => x.Value)
            .Select(x => x.Key)
            .ToArray();

        // Sessions in ascending date order so the conversion pointer only
        // moves forward.
        var orderedSessions = seed
            .Sessions.Where(s => s.Status != SessionStatus.Cancelled)
            .OrderBy(s => s.Date)
            .ToArray();

        // Reused buffers. Each grows at most to the number of converted
        // visitors in this congregation. Cleared and reused across sessions
        // rather than reallocated.
        var convertedSoFar = new List<Guid>(conversions.Length);
        var convertedNotYet = new List<Guid>(conversions.Length);
        convertedNotYet.AddRange(conversions);

        var boundary = 0;

        foreach (var session in orderedSessions)
        {
            var sessionInstant = session.Date.ToDateTime(TimeOnly.MinValue);

            // Anyone whose conversion instant is on or before this session
            // is now a member, not a visitor. Move them once.
            while (
                boundary < conversions.Length
                && seed.ConvertedVisitorDates[conversions[boundary]] <= sessionInstant
            )
            {
                var personId = conversions[boundary];
                convertedSoFar.Add(personId);
                convertedNotYet.RemoveAt(0);
                boundary++;
            }

            var memberCount = AttendeeCountFor(
                session.SessionLabel.Category,
                seed.IsBig,
                alwaysMembers.Count + convertedSoFar.Count,
                rng
            );
            var visitorCount = VisitorCountFor(
                session.SessionLabel.Category,
                seed.IsBig,
                alwaysVisitors.Count + convertedNotYet.Count
            );

            if (memberCount > 0)
            {
                var picks = rng.PickFromPair(alwaysMembers, convertedSoFar, memberCount);
                foreach (var personId in picks)
                {
                    yield return BuildAttendance(
                        seed.Id,
                        session,
                        personId,
                        PersonKind.Member,
                        rng,
                        ctx.Now
                    );
                }
            }

            if (visitorCount > 0)
            {
                var picks = rng.PickFromPair(alwaysVisitors, convertedNotYet, visitorCount);
                foreach (var personId in picks)
                {
                    yield return BuildAttendance(
                        seed.Id,
                        session,
                        personId,
                        PersonKind.Visitor,
                        rng,
                        ctx.Now
                    );
                }
            }
        }
    }

    private static Attendance BuildAttendance(
        Guid congregationId,
        Session session,
        Guid personId,
        PersonKind kind,
        Rng rng,
        DateTime now
    )
    {
        // 60% of attendance is QR-scan style (has a check-in timestamp).
        // The rest are manual entries where only presence matters.
        DateTime? checkedInAt = null;
        if (rng.Chance(0.6) && session.StartsAt is { } start)
        {
            var offsetMinutes = rng.Next(-15, 31);
            var startDateTime = session.Date.ToDateTime(start).AddMinutes(offsetMinutes);
            checkedInAt = DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc);
        }

        return new Attendance
        {
            Id = Guid.NewGuid(),
            CongregationId = congregationId,
            SessionId = session.Id,
            Session = session,
            PersonId = personId,
            PersonKindAtTime = kind,
            CheckedInAt = checkedInAt,
            RecordedBy = null,
            CreatedAt = now,
        };
    }

    private static int AttendeeCountFor(
        SessionCategory category,
        bool isBig,
        int availableMembers,
        Rng rng
    )
    {
        var (min, max) = isBig
            ? category switch
            {
                SessionCategory.Service => (1500, 2500),
                SessionCategory.Meeting => (600, 1000),
                SessionCategory.Study => (400, 700),
                SessionCategory.Prayer => (200, 400),
                SessionCategory.Practice => (80, 200),
                SessionCategory.Outreach => (400, 800),
                SessionCategory.Class => (200, 500),
                SessionCategory.Special => (1200, 2200),
                _ => (300, 600),
            }
            : category switch
            {
                SessionCategory.Service => (80, 140),
                SessionCategory.Meeting => (40, 70),
                SessionCategory.Study => (30, 60),
                SessionCategory.Prayer => (20, 40),
                SessionCategory.Practice => (15, 30),
                SessionCategory.Outreach => (30, 60),
                SessionCategory.Class => (20, 40),
                SessionCategory.Special => (70, 120),
                _ => (25, 50),
            };

        var count = rng.Next(min, max + 1);
        return Math.Min(count, availableMembers);
    }

    private static int VisitorCountFor(SessionCategory category, bool isBig, int availableVisitors)
    {
        if (availableVisitors == 0)
            return 0;

        // Visitors attend proportionally less than members, and less at
        // meetings and classes than at services.
        var ratio = category switch
        {
            SessionCategory.Service => isBig ? 0.05 : 0.12,
            SessionCategory.Special => isBig ? 0.06 : 0.15,
            SessionCategory.Outreach => isBig ? 0.04 : 0.10,
            _ => isBig ? 0.02 : 0.05,
        };

        var count = (int)Math.Round(availableVisitors * ratio);
        return Math.Min(count, availableVisitors);
    }
}
