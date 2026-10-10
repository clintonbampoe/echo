using Echo.Domain.Attendances;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates attendance records for both members and visitors.
///
/// Attendance references persons (not members), so both kinds can check in.
/// The unique index on (CongregationId, PersonId, AttendanceTypeId, Date)
/// means we cannot insert the same person twice for the same service on the
/// same date. We avoid this by tracking (personId, typeId, date) as we generate
/// and skipping duplicates.
///
/// Attendance types are weighted so Sunday Service dominates, matching what
/// real attendance distributions look like.
/// </summary>
public sealed class AttendancePhase : ISeedPhase
{
    public string Name => "Attendance";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.AttendanceTypes.Count == 0 || seed.MemberIds.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no attendance types or members, skipping attendance.",
                    seed.Congregation.Name
                );
                continue;
            }

            var target = seed.IsBig ? 200_000 : 10_000;
            var allPersonIds = seed.AllPersonIds.ToList();
            var weightedTypes = seed.AttendanceTypes.Select(t => (t, WeightFor(t.Name))).ToList();

            // Track generated (personId, typeId, date) to honour the unique index.
            // For large volumes this set can get big — but it's bounded by target count.
            var seen = new HashSet<(Guid, int, DateOnly)>(target);

            var source = GenerateAttendance(
                rng,
                seed.Id,
                allPersonIds,
                weightedTypes,
                target,
                seen,
                ctx.Now
            );

            await CopyBatch.WriteAsync(
                ctx.Connection,
                source,
                ctx.Options.BatchSize,
                AttendanceCopy.WriteAttendancesAsync,
                total =>
                    ctx.Logger.LogInformation(
                        "  {Congregation} attendance: {N}/{Total}",
                        seed.Congregation.Name,
                        total,
                        target
                    ),
                ct
            );
        }
    }

    private static IEnumerable<Attendance> GenerateAttendance(
        Rng rng,
        Guid congregationId,
        IReadOnlyList<Guid> personIds,
        IReadOnlyList<(AttendanceType Type, double Weight)> weightedTypes,
        int target,
        HashSet<(Guid, int, DateOnly)> seen,
        DateTime now
    )
    {
        var attempts = 0;
        var produced = 0;
        // Allow up to 3× attempts to reach the target before giving up,
        // so we don't loop forever if the space is saturated.
        var maxAttempts = target * 3;

        while (produced < target && attempts < maxAttempts)
        {
            attempts++;

            var type = rng.Weighted(weightedTypes);
            var personId = rng.Pick(personIds);
            var date = rng.RecentDate(now, 730);

            // Skip if this combination already exists in this run.
            if (!seen.Add((personId, type.Id, date)))
                continue;

            produced++;
            yield return new Attendance
            {
                Id = Guid.NewGuid(),
                CongregationId = congregationId,
                AttendanceTypeId = type.Id,
                PersonId = personId,
                Date = date,
                CheckInTime = new TimeOnly(rng.Next(7, 12), rng.Next(0, 60)),
                Notes = null,
                CreatedAt = now,
            };
        }
    }

    private static double WeightFor(string typeName) =>
        typeName switch
        {
            "Sunday Service" => 40.0,
            "Midweek Service" => 15.0,
            "Prayer Meeting" => 10.0,
            "Bible Study" => 8.0,
            "Youth Service" => 6.0,
            "Communion Service" => 5.0,
            "Vigil" => 4.0,
            "Cell Group" => 4.0,
            "Morning Devotion" => 3.0,
            _ => 5.0,
        };
}
