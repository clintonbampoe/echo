using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Writes each congregation's events with their registrations and attendance.
/// <br/><br/>
/// The rules live in <see cref="EventPlanner"/>: events come from the ministries
/// the congregation has, led by their leaders, on the same slot every year, and
/// the people who register and come follow their interest, commitment and
/// household. Event names are unique per congregation (unique index on
/// CongregationId + Name), which the year in each name guarantees.
/// <br/><br/>
/// Output written to state: EventIds. Reads Organizations, so it runs after
/// OrganizationsPhase.
/// </summary>
public sealed class EventsPhase : ISeedPhase
{
    public string Name => "Events";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        foreach (var seed in ctx.State)
        {
            if (seed.Members.Count == 0 || seed.Organizations.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members or organizations, skipping events.",
                    seed.Congregation.Name
                );
                continue;
            }

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var planner = new EventPlanner(seed, rng, ctx.Today, ctx.Now, ctx.Options.EventMonths);

            var planned = planner.Plan();
            var events = planned.Select(p => p.Event).ToList();
            var participation = planner.Participate(planned);

            await CopyBatch.WriteAsync(
                ctx.Connection,
                events,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventsAsync,
                null,
                ct
            );
            seed.EventIds.AddRange(events.Select(e => e.Id));

            await CopyBatch.WriteAsync(
                ctx.Connection,
                participation.Registrations,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventRegistrationsAsync,
                null,
                ct
            );

            await CopyBatch.WriteAsync(
                ctx.Connection,
                participation.Attendances,
                ctx.Options.BatchSize,
                EventsCopy.WriteEventAttendancesAsync,
                null,
                ct
            );

            ctx.Logger.LogInformation(
                "{Congregation}: {E} events, {R:N0} registrations, {A:N0} attendance.",
                seed.Congregation.Name,
                events.Count,
                participation.Registrations.Count,
                participation.Attendances.Count
            );
        }
    }
}
