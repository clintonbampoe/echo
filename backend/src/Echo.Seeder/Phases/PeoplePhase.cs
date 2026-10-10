using Echo.Domain.Members;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;

namespace Echo.Seeder.Phases;

/// <summary>
/// Generates Person, Member, and Visitor rows for every congregation.
///
/// Table layout:
///   persons   — shared identity (name, phone, email, kind)
///   members   — extra columns for members (shared PK = PersonId)
///   visitors  — extra columns for visitors (shared PK = PersonId)
///
/// Each person row is written first, then the corresponding member or visitor
/// side-table row. Both writes are in the same batch transaction.
///
/// Output written to state: MemberIds and VisitorIds on each CongregationSeed.
/// These are HashSet&lt;Guid&gt; so later phases can do O(1) membership checks.
/// </summary>
public sealed class PeoplePhase(SeedFakers fakers) : ISeedPhase
{
    public string Name => "People";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            var memberCount = seed.IsBig ? ctx.Options.BigMembers : ctx.Options.SmallMembers;
            var visitorCount = seed.IsBig ? ctx.Options.BigVisitors : ctx.Options.SmallVisitors;

            ctx.Logger.LogInformation(
                "{Congregation}: generating {M} members and {V} visitors.",
                seed.Congregation.Name,
                memberCount,
                visitorCount
            );

            await GenerateMembersAsync(ctx, rng, seed, memberCount, ct);
            await GenerateVisitorsAsync(ctx, rng, seed, visitorCount, ct);

            ctx.Logger.LogInformation(
                "{Congregation}: people done — {M} members, {V} visitors.",
                seed.Congregation.Name,
                seed.MemberIds.Count,
                seed.VisitorIds.Count
            );
        }
    }

    private async Task GenerateMembersAsync(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed,
        int count,
        CancellationToken ct
    )
    {
        // Build persons and members in parallel lists so one batch covers both tables.
        var persons = new List<Person>(ctx.Options.BatchSize);
        var members = new List<Member>(ctx.Options.BatchSize);

        var source = Enumerable
            .Range(0, count)
            .Select(_ =>
            {
                var person = fakers.Person.Generate();
                person.CongregationId = seed.Id;
                person.Kind = PersonKind.Member;
                person.CreatedAt = ctx.Now;

                var member = fakers.Member.Generate();
                member.PersonId = person.Id;
                member.CongregationId = seed.Id;
                // JoinedDate must be on or before ReferenceNow
                member.JoinedDate = rng.RecentDate(ctx.Now, 3650);
                member.CreatedAt = ctx.Now;

                return (person, member);
            });

        await CopyBatch.WriteAsync(
            ctx.Connection,
            source,
            ctx.Options.BatchSize,
            async (conn, batch, innerCt) =>
            {
                var batchPersons = batch.Select(x => x.person).ToList();
                var batchMembers = batch.Select(x => x.member).ToList();

                await PeopleCopy.WritePersonsAsync(conn, batchPersons, innerCt);
                await PeopleCopy.WriteMembersAsync(conn, batchMembers, innerCt);

                foreach (var p in batchPersons)
                    seed.MemberIds.Add(p.Id);
            },
            total => ctx.Logger.LogInformation("  members: {N}/{Total}", total, count),
            ct
        );
    }

    private async Task GenerateVisitorsAsync(
        SeederContext ctx,
        Rng rng,
        CongregationSeed seed,
        int count,
        CancellationToken ct
    )
    {
        var source = Enumerable
            .Range(0, count)
            .Select(_ =>
            {
                // How long ago they first visited. 0–5 years back.
                var visitDaysAgo = rng.Next(0, 365 * 5);
                var firstVisitAt = ctx.Now.AddDays(-visitDaysAgo);

                // First visit is always in the past.
                var person = fakers.Person.Generate();
                person.CongregationId = seed.Id;
                person.CreatedAt = firstVisitAt;

                var converted = rng.Chance(0.10);

                Member? member = null;
                DateTime? convertedAt = null;

                if (converted)
                {
                    // They converted somewhere between their first visit and now.
                    // If they visited 10 days ago, conversion happened in [0, 10] days ago.
                    var conversionDaysAgo = rng.Next(0, visitDaysAgo + 1);
                    convertedAt = ctx.Now.AddDays(-conversionDaysAgo);

                    person.Kind = PersonKind.Member;

                    member = fakers.Member.Generate();
                    member.PersonId = person.Id;
                    member.CongregationId = seed.Id;
                    member.CreatedAt = convertedAt.Value;
                    member.JoinedDate = DateOnly.FromDateTime(convertedAt.Value);
                }
                else
                {
                    person.Kind = PersonKind.Visitor;
                }

                var visitor = new Visitor
                {
                    PersonId = person.Id,
                    CongregationId = seed.Id,
                    Notes = null,
                    ConvertedAt = convertedAt,
                    CreatedAt = firstVisitAt,
                };

                return (person, member, visitor);
            });

        await CopyBatch.WriteAsync(
            ctx.Connection,
            source,
            ctx.Options.BatchSize,
            async (conn, batch, innerCt) =>
            {
                var batchPersons = batch.Select(x => x.person).ToList();
                var batchMembers = batch
                    .Where(x => x.member is not null)
                    .Select(x => x.member!)
                    .ToList();
                var batchVisitors = batch.Select(x => x.visitor).ToList();

                // Order matters: Person first (shared PK), then the side tables.
                await PeopleCopy.WritePersonsAsync(conn, batchPersons, innerCt);

                if (batchMembers.Count > 0)
                    await PeopleCopy.WriteMembersAsync(conn, batchMembers, innerCt);

                await PeopleCopy.WriteVisitorsAsync(conn, batchVisitors, innerCt);

                foreach (var p in batchPersons)
                    seed.VisitorIds.Add(p.Id);

                foreach (var m in batchMembers)
                    seed.MemberIds.Add(m.PersonId);
            },
            total => ctx.Logger.LogInformation("  visitors: {N}/{Total}", total, count),
            ct
        );
    }
}
