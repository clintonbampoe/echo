using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Writes each congregation's calendar: weekly services and midweek meetings,
/// monthly meetings, and the days of the church year.
/// <br/><br/>
/// The calendar covers <c>AttendanceWeeks</c> back from ReferenceNow and
/// <c>FutureWeeks</c> forward; see <see cref="SessionPlanner"/> for what it
/// contains. The big congregation holds two Sunday services, the others one.
/// <br/><br/>
/// Output written to state: Sessions and ServiceSlots. AttendancePhase reads
/// them, and the sessions must exist in the database first because attendance
/// rows reference them.
/// </summary>
public sealed class SessionsPhase : ISeedPhase
{
    public string Name => "Sessions";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
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

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var planner = new SessionPlanner(
                rng,
                seed.SessionLabels.ToDictionary(l => l.Category),
                seed.Id,
                seed.Admin.Id,
                seed.Founded,
                ctx.Today,
                ctx.Now,
                ctx.Options.AttendanceWeeks,
                ctx.Options.FutureWeeks,
                twoServices: seed.IsBig
            );

            var planned = planner.Build();
            var sessions = planned.Select(p => p.Session).ToList();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                sessions,
                ctx.Options.BatchSize,
                SessionsCopy.WriteSessionsAsync,
                null,
                ct
            );

            seed.Sessions.AddRange(sessions);
            foreach (var p in planned.Where(p => p.Slot > 0))
                seed.ServiceSlots[p.Session.Id] = p.Slot;

            ctx.Logger.LogInformation(
                "{Congregation}: {N} sessions from {From} to {To}.",
                seed.Congregation.Name,
                sessions.Count,
                sessions[0].Date,
                sessions[^1].Date
            );
        }
    }
}
