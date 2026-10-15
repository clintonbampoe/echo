using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Writes who attended which session, for members and visitors.
/// <br/><br/>
/// The rules live in <see cref="AttendanceSimulator"/>: attendance follows each
/// person's commitment, the household they belong to, the season, and what the
/// session is for. Nobody attends before they joined or after they left, and a
/// visitor who becomes a member is recorded as a visitor up to the day they
/// join and as a member after.
/// <br/><br/>
/// Rows are produced lazily and streamed straight into COPY, so a million rows
/// never sit in memory at once.
/// </summary>
public sealed class AttendancePhase : ISeedPhase
{
    public string Name => "Attendance";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        foreach (var seed in ctx.State)
        {
            if (seed.Sessions.Count == 0 || seed.Members.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no sessions or members, skipping attendance.",
                    seed.Congregation.Name
                );
                continue;
            }

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var simulator = new AttendanceSimulator(seed, rng, ctx.Now);

            await CopyBatch.WriteAsync(
                ctx.Connection,
                simulator.Generate(),
                ctx.Options.BatchSize,
                AttendanceCopy.WriteAttendancesAsync,
                n =>
                {
                    if (n % (ctx.Options.BatchSize * 50) == 0)
                        ctx.Logger.LogInformation(
                            "  {Congregation} attendance: {N}",
                            seed.Congregation.Name,
                            n
                        );
                },
                ct
            );
        }
    }
}
