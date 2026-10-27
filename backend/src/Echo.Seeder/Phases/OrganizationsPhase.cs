using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Creates the congregation's ministries and decides who belongs to each.
/// <br/><br/>
/// Ministries come from <see cref="OrganizationCatalog"/>: every congregation
/// has the core ones (choir, ushers, children, youth, women, men, prayer) and
/// the big one has most of the rest. Core ministries start early in the
/// congregation's history; the others are added later.
/// <br/><br/>
/// Membership follows the audience (a women's fellowship has women, the
/// children's ministry has children), the ministry's usual reach, and each
/// person's commitment: committed people join more. Nobody joins before they
/// joined the congregation or before the ministry existed. The leader and
/// secretary are committed adults who are still around.
/// <br/><br/>
/// Output written to state: Organizations (with member ids) and OrganizationIds.
/// </summary>
public sealed class OrganizationsPhase : ISeedPhase
{
    public string Name => "Organizations";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        foreach (var seed in ctx.State)
        {
            if (seed.Members.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members, skipping organizations.",
                    seed.Congregation.Name
                );
                continue;
            }

            var rng = ctx.RngFor(Name, seed.Congregation.Name);
            var kinds = ChooseKinds(seed, rng);

            var organizations = new List<Organization>(kinds.Count);
            var memberships = new List<OrganizationMember>();

            foreach (var kind in kinds)
            {
                var founded = FoundedOn(ctx, seed, kind, rng);

                // A congregation with nobody the right age skips that ministry.
                var roster = BuildRoster(ctx, seed, kind, founded, rng);
                if (roster.Count == 0)
                    continue;

                var type = OrganizationCatalog.TypeOf(kind.Name);
                var organization = new Organization
                {
                    Id = rng.NewGuid(),
                    CongregationId = seed.Id,
                    Name = kind.Name,
                    Description = kind.Description,
                    Kind = type,
                    MeetingDay = OrganizationCatalog.MeetingDayOf(type),
                    CreatedAt = Stamps.On(founded, rng, ctx.Now),
                };
                organizations.Add(organization);

                memberships.AddRange(ToRows(ctx, seed, organization, roster, rng));

                seed.Organizations.Add(
                    new OrganizationProfile(
                        organization.Id,
                        organization.Name,
                        founded,
                        roster[0].Member.Id,
                        [.. roster.Select(r => r.Member.Id)]
                    )
                );
                seed.OrganizationIds.Add(organization.Id);
            }

            await CopyBatch.WriteAsync(
                ctx.Connection,
                organizations,
                ctx.Options.BatchSize,
                OrganizationsCopy.WriteOrganizationsAsync,
                null,
                ct
            );

            await CopyBatch.WriteAsync(
                ctx.Connection,
                memberships,
                ctx.Options.BatchSize,
                OrganizationsCopy.WriteOrganizationMembersAsync,
                null,
                ct
            );

            ctx.Logger.LogInformation(
                "{Congregation}: {Orgs} organizations, {Memberships} memberships.",
                seed.Congregation.Name,
                organizations.Count,
                memberships.Count
            );
        }
    }

    private sealed record Placement(PersonProfile Member, MemberRole Role, DateOnly JoinedAt);

    // The core ministries first, then as many others as the congregation's size supports.
    private static List<OrganizationKind> ChooseKinds(CongregationSeed seed, Rng rng)
    {
        var core = OrganizationCatalog.All.Where(k => k.Core).ToList();
        var others = OrganizationCatalog.All.Where(k => !k.Core).ToList();

        var wanted = seed.IsBig
            ? rng.Next(30, 37)
            : Math.Clamp(6 + seed.Members.Count / 120 + rng.Next(-2, 3), core.Count, 22);

        var extra = rng.Shuffle(others).Take(Math.Max(0, wanted - core.Count));
        return [.. core, .. extra];
    }

    // Core ministries are as old as the congregation; the others come later.
    private static DateOnly FoundedOn(
        SeederContext ctx,
        CongregationSeed seed,
        OrganizationKind kind,
        Rng rng
    )
    {
        var latest = ctx.Today.AddDays(-90);
        var earliest = seed.Founded.AddDays(kind.Core ? 0 : 365);
        if (earliest >= latest)
            return seed.Founded;

        var span = latest.DayNumber - earliest.DayNumber;
        var skew = kind.Core ? 3.0 : 1.0;
        var offset = (int)(span * Math.Pow(rng.NextDouble(), skew));
        return earliest.AddDays(offset);
    }

    // Picks the members, leader first, then the secretary, then everyone else.
    private static List<Placement> BuildRoster(
        SeederContext ctx,
        CongregationSeed seed,
        OrganizationKind kind,
        DateOnly founded,
        Rng rng
    )
    {
        var today = ctx.Today;
        var candidates = new List<(PersonProfile Member, DateOnly JoinedAt, double Key)>();
        var activeEligible = 0;

        foreach (var member in seed.Members)
        {
            var age = member.AgeOn(today);
            if (!OrganizationCatalog.Fits(kind.Audience, age, member.Gender))
                continue;

            var joinedAt = Later(founded, member.JoinedOn.AddDays(rng.Next(14, 500)));
            if (joinedAt > today || (member.LeftOn is { } left && joinedAt >= left))
                continue;

            if (member.LeftOn is null)
                activeEligible++;

            // Committed people join more; people who have left count for less.
            var weight =
                (0.15 + 2.0 * member.Commitment * member.Commitment)
                * (member.LeftOn is null ? 1.0 : 0.4);
            candidates.Add((member, joinedAt, Math.Pow(rng.NextDouble(), 1.0 / weight)));
        }

        var target = (int)Math.Round(activeEligible * kind.Reach * 1.1);
        target = Math.Clamp(target, Math.Min(3, candidates.Count), candidates.Count);

        if (candidates.Count == 0)
            return [];

        var chosen = candidates.OrderByDescending(c => c.Key).Take(target).ToList();
        return Assign(ctx, seed, kind, founded, chosen, rng);
    }

    private static List<Placement> Assign(
        SeederContext ctx,
        CongregationSeed seed,
        OrganizationKind kind,
        DateOnly founded,
        List<(PersonProfile Member, DateOnly JoinedAt, double Key)> chosen,
        Rng rng
    )
    {
        var today = ctx.Today;

        // Leaders of children's and teens' ministries are adults from outside the audience.
        var leadersPool = kind.Audience is Audience.Children or Audience.Teens
            ? seed
                .Members.Where(m => m.LeftOn is null && m.AgeOn(today) >= 25)
                .OrderByDescending(m => m.Commitment + rng.Gaussian(0, 0.05))
                .Take(20)
                .Select(m => (Member: m, JoinedAt: Later(founded, m.JoinedOn)))
                .ToList()
            : chosen
                .Where(c => c.Member.LeftOn is null && c.Member.AgeOn(today) >= 21)
                .OrderByDescending(c => c.Member.Commitment + rng.Gaussian(0, 0.05))
                .Take(20)
                .Select(c => (c.Member, c.JoinedAt))
                .ToList();

        if (leadersPool.Count == 0)
            leadersPool = [(chosen[0].Member, chosen[0].JoinedAt)];

        var leader = leadersPool[rng.Next(Math.Min(5, leadersPool.Count))];
        var secretary = leadersPool.FirstOrDefault(p => p.Member.Id != leader.Member.Id);

        var roster = new List<Placement>(chosen.Count + 2)
        {
            new(leader.Member, MemberRole.Leader, leader.JoinedAt),
        };

        if (secretary.Member is not null)
            roster.Add(new Placement(secretary.Member, MemberRole.Secretary, secretary.JoinedAt));

        var taken = roster.Select(r => r.Member.Id).ToHashSet();
        roster.AddRange(
            chosen
                .Where(c => !taken.Contains(c.Member.Id))
                .Select(c => new Placement(c.Member, MemberRole.Member, c.JoinedAt))
        );

        return roster;
    }

    private static IEnumerable<OrganizationMember> ToRows(
        SeederContext ctx,
        CongregationSeed seed,
        Organization organization,
        List<Placement> roster,
        Rng rng
    ) =>
        roster.Select(p => new OrganizationMember
        {
            Id = rng.NewGuid(),
            CongregationId = seed.Id,
            OrganizationId = organization.Id,
            MemberId = p.Member.Id,
            Role = p.Role,
            JoinedAt = p.JoinedAt,
            CreatedAt = Stamps.On(p.JoinedAt, rng, ctx.Now),
        });

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;
}
