using Echo.Domain.Attendances;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates attendance sessions for each congregation.
/// <br/><br/>
/// Every congregation gets sessions for each of its labels, going back
/// 26 weeks and forward 4 weeks from ReferenceNow. Weekly labels produce
/// roughly 30 sessions; monthly, biweekly, and quarterly labels produce
/// fewer.
/// <br/><br/>
/// Session status is inferred from the date: past sessions are Completed
/// (with a small chance of Cancelled), future sessions are Scheduled.
/// <br/><br/>
/// Output written to state: Sessions on each CongregationSeed. The
/// AttendancePhase reads these to generate attendance rows. Sessions are
/// also written to the database here — Attendance rows reference them
/// through a foreign key, so they must exist before AttendancePhase runs.
/// </summary>
public sealed class SessionsPhase : ISeedPhase
{
    public string Name => "Sessions";

    private const int _weeksBack = 26;
    private const int _weeksForward = 4;

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.SessionLabels.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no session labels, skipping sessions.",
                    seed.Congregation.Name
                );
                continue;
            }

            var startDate = DateOnly.FromDateTime(ctx.Now.AddDays(-_weeksBack * 7));
            var endDate = DateOnly.FromDateTime(ctx.Now.AddDays(_weeksForward * 7));
            var today = DateOnly.FromDateTime(ctx.Now);

            var sessions = new List<Session>();

            foreach (var label in seed.SessionLabels)
            {
                var cadenceDays = CadenceFor(label.Category);

                for (var date = startDate; date <= endDate; date = date.AddDays(cadenceDays))
                {
                    var isPast = date < today;
                    var status = SessionStatus.Scheduled;
                    if (isPast)
                        status = rng.Chance(0.03)
                            ? SessionStatus.Cancelled
                            : SessionStatus.Completed;

                    sessions.Add(
                        new Session
                        {
                            Id = Guid.NewGuid(),
                            CongregationId = seed.Id,
                            SessionLabelId = label.Id,
                            SessionLabel = label,
                            Date = date,
                            Name = null,
                            Topic = null,
                            Notes = null,
                            StartsAt = TimeFor(label.Category, rng),
                            EndsAt = null,
                            Status = status,
                            CreatedBy = null,
                            CreatedAt = ctx.Now,
                        }
                    );
                }
            }

            await CopyBatch.WriteAsync(
                ctx.Connection,
                sessions,
                ctx.Options.BatchSize,
                SessionsCopy.WriteSessionsAsync,
                null,
                ct
            );

            seed.Sessions.AddRange(sessions);

            ctx.Logger.LogInformation(
                "{Congregation}: {N} sessions across {L} labels.",
                seed.Congregation.Name,
                sessions.Count,
                seed.SessionLabels.Count
            );
        }
    }

    private static int CadenceFor(SessionCategory category) =>
        category switch
        {
            SessionCategory.Service => 7,
            SessionCategory.Meeting => 7,
            SessionCategory.Study => 7,
            SessionCategory.Prayer => 7,
            SessionCategory.Practice => 7,
            SessionCategory.Outreach => 30,
            SessionCategory.Class => 14,
            SessionCategory.Special => 90,
            _ => 30,
        };

    private static TimeOnly? TimeFor(SessionCategory category, Rng rng) =>
        category switch
        {
            SessionCategory.Service => new TimeOnly(rng.Next(7, 11), 0),
            SessionCategory.Meeting => new TimeOnly(rng.Next(17, 20), 0),
            SessionCategory.Study => new TimeOnly(rng.Next(17, 20), 0),
            SessionCategory.Prayer => new TimeOnly(rng.Next(5, 7), 0),
            SessionCategory.Practice => new TimeOnly(rng.Next(16, 19), 0),
            SessionCategory.Outreach => new TimeOnly(rng.Next(8, 15), 0),
            SessionCategory.Class => new TimeOnly(rng.Next(17, 20), 0),
            SessionCategory.Special => new TimeOnly(rng.Next(8, 18), 0),
            _ => new TimeOnly(10, 0),
        };
}
