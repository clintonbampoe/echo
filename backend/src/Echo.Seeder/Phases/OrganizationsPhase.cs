using Echo.Domain.Organizations;
using Echo.Seeder.Copy;
using Echo.Seeder.Core;

namespace Echo.Seeder.Phases;

/// <summary>
/// Creates organizations and assigns members to them.
/// <br/> <br/>
/// Each org gets one Leader, one optional Secretary, and the rest are Members.
/// A member can belong to more than one org.
///
/// <br/><br/>
/// Output written to state: OrganizationIds on each CongregationSeed.
/// </summary>
public sealed class OrganizationsPhase() : ISeedPhase
{
    public string Name => "Organizations";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);

        foreach (var seed in ctx.State)
        {
            if (seed.MemberIds.Count == 0)
            {
                ctx.Logger.LogWarning(
                    "{Congregation}: no members, skipping organizations.",
                    seed.Congregation.Name
                );
                continue;
            }

            var orgCount = seed.IsBig ? rng.Next(50, 100) : rng.Next(5, 50);
            var membersPerOrg = seed.IsBig ? rng.Next(50, 20000) : rng.Next(10, 500);
            var memberList = seed.MemberIds.ToList(); // snapshot for shuffling

            if (orgCount > _orgNamePool.Count)
            {
                throw new InvalidOperationException(
                    $"Requested {orgCount} organizations but the name pool only has "
                        + $"{_orgNamePool.Count} combinations. Add more names or suffixes."
                );
            }

            // ── Organizations ─────────────────────────────────────────────
            // Draw names without replacement — uniqueness by construction.
            // Shuffle gets a mutable copy so the static pool stays untouched.
            var orgNames = rng.Shuffle(_orgNamePool.ToList()).Take(orgCount).ToList();

            var orgs = orgNames
                .Select(name => new Organization
                {
                    Id = Guid.NewGuid(),
                    CongregationId = seed.Id,
                    Name = name,
                    Description = rng.Chance(0.5) ? rng.Pick(_orgPurposes) : null,
                    CreatedAt = ctx.Now,
                })
                .ToList();

            await CopyBatch.WriteAsync(
                ctx.Connection,
                orgs,
                ctx.Options.BatchSize,
                OrganizationsCopy.WriteOrganizationsAsync,
                null,
                ct
            );

            foreach (var org in orgs)
                seed.OrganizationIds.Add(org.Id);

            // ── OrganizationMembers ───────────────────────────────────────
            var assignments = new List<OrganizationMember>();

            foreach (var org in orgs)
            {
                // Fresh copy per org so Shuffle doesn't mutate the shared list
                // (and so each org draws from the same full pool).
                var chosen = rng.Shuffle(memberList.ToList())
                    .Take(Math.Min(membersPerOrg, memberList.Count));

                var first = true;
                foreach (var memberId in chosen)
                {
                    var role =
                        first ? MemberRole.Leader
                        : rng.Chance(0.10) ? MemberRole.Secretary
                        : MemberRole.Member;
                    first = false;

                    assignments.Add(
                        new OrganizationMember
                        {
                            Id = Guid.NewGuid(),
                            CongregationId = seed.Id,
                            OrganizationId = org.Id,
                            MemberId = memberId,
                            Role = role,
                            JoinedAt = rng.RecentDate(ctx.Now, 1000),
                            CreatedAt = ctx.Now,
                        }
                    );
                }
            }

            await CopyBatch.WriteAsync(
                ctx.Connection,
                assignments,
                ctx.Options.BatchSize,
                OrganizationsCopy.WriteOrganizationMembersAsync,
                null,
                ct
            );

            ctx.Logger.LogInformation(
                "{Congregation}: {Orgs} orgs, {Assignments} memberships.",
                seed.Congregation.Name,
                orgs.Count,
                assignments.Count
            );
        }
    }

    // ── Reference data ────────────────────────────────────────────────────

    private static readonly IReadOnlyList<string> _orgNames =
    [
        // Worship & music
        "Choir",
        "Praise Team",
        "Worship Team",
        "Music",
        "Instrumentalists",
        "Drama",
        "Dance",
        "Media",
        "Sound",
        "Audio-Visual",
        // Welcome & care
        "Ushering",
        "Protocol",
        "Welcome",
        "Hospitality",
        "Welfare",
        "Member Care",
        "Visitation",
        "Bereavement",
        "Counseling",
        // Teaching & discipleship
        "Children's",
        "Sunday School",
        "Teens",
        "Youth",
        "Young Adults",
        "Adults",
        "Bible Study",
        "Discipleship",
        "Catechism",
        "Training",
        // Prayer & spiritual
        "Prayer",
        "Intercession",
        "Deliverance",
        "Fasting",
        "Prophetic",
        "Healing",
        // Outreach & missions
        "Evangelism",
        "Missions",
        "Outreach",
        "Community Service",
        "Charity",
        "Street Ministry",
        "Prison Ministry",
        "Hospital Ministry",
        // Fellowship groups
        "Men's Fellowship",
        "Women's Fellowship",
        "Marriage",
        "Singles",
        "Elders",
        "Deacons",
        // Operations
        "Finance",
        "Stewardship",
        "Logistics",
        "Maintenance",
        "Security",
        "Transport",
        "Events",
        "Publicity",
    ];

    private static readonly IReadOnlyList<string> _orgSuffixes =
    [
        "Team",
        "Ministry",
        "Department",
        "Committee",
        "Group",
        "Unit",
        "Board",
        "Council",
        "Fellowship",
        "Circle",
        "Band",
    ];

    /// <summary>
    /// The full cartesian product of (_orgNames × _orgSuffixes).
    /// Current size: 57 × 11 = 627 unique combinations.
    /// Names are drawn from this pool WITHOUT replacement, so uniqueness
    /// within a congregation is guaranteed by construction.
    /// If orgCount ever approaches this size, add another dimension
    /// (e.g. themes) or split into separate pools.
    /// </summary>
    private static readonly IReadOnlyList<string> _orgNamePool =
    [
        .. (from name in _orgNames from suffix in _orgSuffixes select $"{name} {suffix}"),
    ];

    private static readonly IReadOnlyList<string> _orgPurposes =
    [
        "Coordinates Sunday worship.",
        "Handles visitor welcome.",
        "Runs congregation events.",
        "Provides member care.",
        "Organizes outreach activities.",
    ];
}
