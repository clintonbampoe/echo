using Bogus;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Simulates the people of every congregation: who joined when, in which
/// household, who left, who visited and who of those visitors became members.
///
/// <br/><br/>
/// The simulation runs in memory (see <see cref="PeopleFactory"/>) and the
/// result is written with COPY: persons first, then the members and visitors
/// that reference them, in one transaction per batch.
///
/// <br/><br/>
/// Reads Founded, which SetupPhase decided. Output written to state: Members
/// and Visitors (the profiles that later phases read), MemberIds, VisitorIds
/// and ConvertedVisitorDates.
///
/// <br/><br/>
/// BigMembers and SmallMembers count the members from households.
/// Visitors who converted are extra, so a congregation ends up with slightly
/// more members than configured.
/// </summary>
public sealed class PeoplePhase : ISeedPhase
{
    public string Name => "People";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        foreach (var seed in ctx.State)
        {
            // One stream per congregation, so changing one congregation's size
            // never changes the people of another.
            var rng = ctx.RngFor(Name, seed.Congregation.Name);

            var (memberCount, visitorCount) = SizeFor(ctx.Options, seed, rng);
            var growth = GrowthFor(ctx, rng, seed);
            var faker = new Faker("en") { Random = new Randomizer(rng.Next(int.MaxValue)) };
            var factory = new PeopleFactory(rng, faker, seed.Congregation, growth, ctx.Now);

            ctx.Logger.LogInformation(
                "{Congregation}: founded {Founded}, simulating {M} members and {V} visitors.",
                seed.Congregation.Name,
                seed.Founded,
                memberCount,
                visitorCount
            );

            await WriteMembersAsync(ctx, factory, seed, memberCount, ct);
            await WriteVisitorsAsync(ctx, factory, seed, visitorCount, ct);

            var active = seed.Members.Count(m => m.LeftOn is null);
            var converted = seed.Visitors.Count(v => v.ConvertedOn is not null);

            ctx.Logger.LogInformation(
                "{Congregation}: people done — {M} members ({Active} active), {V} visitors ({Converted} converted).",
                seed.Congregation.Name,
                seed.Members.Count,
                active,
                seed.Visitors.Count,
                converted
            );
        }
    }

    // Small congregations differ in size, so the data does not look cloned.
    private static (int Members, int Visitors) SizeFor(
        SeederOptions options,
        CongregationSeed seed,
        Rng rng
    )
    {
        if (seed.IsBig)
            return (options.BigMembers, options.BigVisitors);

        var scale = rng.Range(1 - options.SmallSizeSpread, 1 + options.SmallSizeSpread);
        var members = (int)Math.Max(20, Math.Round(options.SmallMembers * scale));
        var visitors = (int)Math.Round(options.SmallVisitors * scale);
        return (members, visitors);
    }

    // Founded is decided in SetupPhase, so the congregation and its admin are as
    // old as its members. A small congregation grows at its own pace.
    private static GrowthCurve GrowthFor(SeederContext ctx, Rng rng, CongregationSeed seed)
    {
        var growth = seed.IsBig ? 0.14 : rng.Range(0.08, 0.2);
        return new GrowthCurve(seed.Founded, ctx.Today, growth);
    }

    private static async Task WriteMembersAsync(
        SeederContext ctx,
        PeopleFactory factory,
        CongregationSeed seed,
        int count,
        CancellationToken ct
    )
    {
        await CopyBatch.WriteAsync(
            ctx.Connection,
            MembersOf(factory, count),
            ctx.Options.BatchSize,
            async (conn, batch, innerCt) =>
            {
                await PeopleCopy.WritePersonsAsync(
                    conn,
                    batch.Select(x => x.Person).ToList(),
                    innerCt
                );
                await PeopleCopy.WriteMembersAsync(
                    conn,
                    batch.Select(x => x.Member).ToList(),
                    innerCt
                );

                foreach (var created in batch)
                    Register(seed, created.Profile);
            },
            total => ctx.Logger.LogInformation("  members: {N}/{Total}", total, count),
            ct
        );
    }

    private static async Task WriteVisitorsAsync(
        SeederContext ctx,
        PeopleFactory factory,
        CongregationSeed seed,
        int count,
        CancellationToken ct
    )
    {
        var source = Enumerable.Range(0, count).Select(_ => factory.BuildVisitor());

        await CopyBatch.WriteAsync(
            ctx.Connection,
            source,
            ctx.Options.BatchSize,
            async (conn, batch, innerCt) =>
            {
                // Order matters: Person first (shared PK), then the side tables.
                await PeopleCopy.WritePersonsAsync(
                    conn,
                    batch.Select(x => x.Person).ToList(),
                    innerCt
                );

                var members = batch
                    .Where(x => x.Member is not null)
                    .Select(x => x.Member!)
                    .ToList();
                if (members.Count > 0)
                    await PeopleCopy.WriteMembersAsync(conn, members, innerCt);

                await PeopleCopy.WriteVisitorsAsync(
                    conn,
                    batch.Select(x => x.Visitor).ToList(),
                    innerCt
                );

                foreach (var created in batch)
                {
                    seed.Visitors.Add(created.Story);
                    seed.VisitorIds.Add(created.Story.Id);

                    if (created.Profile is null)
                        continue;

                    Register(seed, created.Profile);
                    seed.ConvertedVisitorDates[created.Story.Id] = created
                        .Visitor
                        .ConvertedAt!
                        .Value;
                }
            },
            total => ctx.Logger.LogInformation("  visitors: {N}/{Total}", total, count),
            ct
        );
    }

    // Households are generated whole and the last one is cut off at the target
    // count, so the number of members is exactly what was asked for.
    private static IEnumerable<NewMember> MembersOf(PeopleFactory factory, int count)
    {
        var made = 0;

        while (made < count)
        {
            foreach (var member in factory.BuildHousehold())
            {
                if (made == count)
                    yield break;

                made++;
                yield return member;
            }
        }
    }

    private static void Register(CongregationSeed seed, PersonProfile profile)
    {
        seed.Members.Add(profile);
        seed.MemberIds.Add(profile.Id);
    }
}
