using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Domain.Users;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;
using Echo.Seeder.Simulation;

namespace Echo.Seeder.Phases;

/// <summary>
/// Creates congregations, admin users, and all lookup tables.
/// <br/><br/>
/// Why EF here instead of COPY?
///   These tables have int PKs with DB-generated sequences. After
///   SaveChangesAsync, EF populates those IDs back onto the entities. Later
///   phases use those IDs as FKs in their COPY statements. There's no safe way
///   to do this with raw COPY without a second round-trip to read the IDs back.
///   Volume is tiny (a handful of rows per congregation) so EF is fine.
/// <br/><br/>
/// This phase also decides when each congregation began (<c>Founded</c>). The
/// congregation, its admin and its lookup tables are created on that day, so
/// the oldest record in the database is as old as the oldest member.
/// <br/><br/>
/// What this produces (written into each CongregationSeed):
///   Congregation, Admin, Founded, SessionLabels, AssetCategories,
///   ProjectCategories, TransactionCategories
/// <br/><br/>
/// Lookup defaults come from CongregationDefaults, the same source the
/// application's registration flow uses. Seeded congregations and
/// newly-registered ones therefore start from identical lookup sets.
/// <br/><br/>
/// Admin accounts are predictable so they are easy to sign in with:
/// <c>admin@{congregation name without spaces}.org</c>, all with the same password.
/// </summary>
public sealed class SetupPhase(SeedFakers fakers) : ISeedPhase
{
    private const string _bigName = "Grace Chapel International";

    public string Name => "Setup";

    // One congregation before it is saved, with the random stream that made it.
    private sealed record Plan(Congregation Congregation, DateOnly Founded, bool IsBig, Rng Rng);

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var plans = PlanCongregations(ctx);
        var congregations = plans.Select(p => p.Congregation).ToList();

        await ctx.Db.Congregations.AddRangeAsync(congregations, ct);
        await ctx.Db.SaveChangesAsync(ct);
        ctx.Logger.CreatedCongregations(congregations.Count, ctx.Options.SmallCount);

        var admins = plans.Select(p => MakeAdmin(p, ctx.Now)).ToList();
        await ctx.Db.Users.AddRangeAsync(admins, ct);
        await ctx.Db.SaveChangesAsync(ct);
        ctx.Logger.CreatedAdmins(admins.Count);

        // Lookup tables
        // Every congregation gets the same defaults, sourced from
        // CongregationDefaults. This matches what the registration flow
        // creates for a brand-new congregation.

        var sessionLabels = new List<SessionLabel>();
        var assetCategories = new List<AssetCategory>();
        var projectCategories = new List<ProjectCategory>();
        var transactionCategories = new List<TransactionCategory>();

        foreach (var congregation in congregations)
        {
            var createdAt = congregation.CreatedAt;

            foreach (var (name, category) in CongregationDefaults.SessionLabels)
                sessionLabels.Add(
                    new SessionLabel
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        Category = category,
                        CreatedAt = createdAt,
                    }
                );

            foreach (var name in CongregationDefaults.AssetCategoryNames)
                assetCategories.Add(
                    new AssetCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CreatedAt = createdAt,
                    }
                );

            foreach (var name in CongregationDefaults.ProjectCategoryNames)
                projectCategories.Add(
                    new ProjectCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CreatedAt = createdAt,
                    }
                );

            foreach (var (name, type) in CongregationDefaults.TransactionCategories)
                transactionCategories.Add(
                    new TransactionCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CategoryType = type,
                        CreatedAt = createdAt,
                    }
                );
        }

        await ctx.Db.SessionLabels.AddRangeAsync(sessionLabels, ct);
        await ctx.Db.AssetCategories.AddRangeAsync(assetCategories, ct);
        await ctx.Db.ProjectCategories.AddRangeAsync(projectCategories, ct);
        await ctx.Db.TransactionCategories.AddRangeAsync(transactionCategories, ct);
        await ctx.Db.SaveChangesAsync(ct);

        ctx.Logger.CreatedReferenceEntities(
            sessionLabels.Count,
            assetCategories.Count,
            projectCategories.Count,
            transactionCategories.Count
        );

        // Populate state
        for (var i = 0; i < plans.Count; i++)
        {
            var congregation = congregations[i];
            var seed = new CongregationSeed
            {
                Congregation = congregation,
                IsBig = plans[i].IsBig,
                Admin = admins[i],
                Founded = plans[i].Founded,
            };

            seed.SessionLabels.AddRange(
                sessionLabels.Where(l => l.CongregationId == congregation.Id)
            );
            seed.AssetCategories.AddRange(
                assetCategories.Where(c => c.CongregationId == congregation.Id)
            );
            seed.ProjectCategories.AddRange(
                projectCategories.Where(c => c.CongregationId == congregation.Id)
            );
            seed.TransactionCategories.AddRange(
                transactionCategories.Where(c => c.CongregationId == congregation.Id)
            );

            ctx.State.Add(seed);
        }
    }

    // ── Congregations ─────────────────────────────────────────────────────

    private List<Plan> PlanCongregations(SeederContext ctx)
    {
        var names = new List<(string Name, bool IsBig)> { (_bigName, true) };
        names.AddRange(
            SmallNames(ctx.RngFor(Name), ctx.Options.SmallCount).Select(n => (n, false))
        );

        return
        [
            .. names.Select(n =>
            {
                // One stream per congregation, so adding or removing one never
                // changes the others.
                var rng = ctx.RngFor(Name, n.Name);
                var founded = Founding.FoundedOn(rng, ctx.Options, ctx.Today, n.IsBig);
                var congregation = MakeCongregation(rng, n.Name, n.IsBig, founded, ctx.Now);
                return new Plan(congregation, founded, n.IsBig, rng);
            }),
        ];
    }

    private Congregation MakeCongregation(
        Rng rng,
        string name,
        bool isBig,
        DateOnly founded,
        DateTime now
    )
    {
        var slug = Slug(name);
        var region = rng.Weighted(_weightedRegions);

        return new Congregation
        {
            Id = rng.NewGuid(),
            Name = name,
            OrgType = CongregationType.Church,
            PhoneNumber = GhanaPhone(rng),
            EmailAddress = $"info@{slug}.org",
            Region = region,
            City = fakers.Raw.Address.City(),
            Town = fakers.Raw.Address.City(),
            GpsAddress = GpsAddress(rng, region),
            PostalAddress = isBig || rng.Chance(0.35) ? $"P.O. Box {rng.Next(100, 9999)}" : null,
            WebsiteUrl = isBig || rng.Chance(0.3) ? $"https://www.{slug}.org" : null,
            CreatedAt = Stamps.On(founded, rng, now),
        };
    }

    // The admin signed up with the congregation and verified their email soon after.
    private User MakeAdmin(Plan plan, DateTime now)
    {
        var user = fakers.User.Generate();
        var createdAt = plan.Congregation.CreatedAt.AddMinutes(2);

        user.Id = plan.Rng.NewGuid();
        user.CongregationId = plan.Congregation.Id;
        user.EmailAddress = $"admin@{Slug(plan.Congregation.Name)}.org";
        user.CreatedAt = createdAt;

        var verifiedAt = createdAt.AddMinutes(plan.Rng.Next(3, 90));
        user.EmailVerifiedAt = verifiedAt > now ? now : verifiedAt;

        return user;
    }

    // Every name is different: a stem and a suffix, each pair used at most once.
    private static List<string> SmallNames(Rng rng, int count)
    {
        var all = _stems
            .SelectMany(stem => _suffixes.Select(suffix => $"{stem} {suffix}"))
            .ToList();
        return [.. rng.Shuffle(all).Take(count)];
    }

    private static string Slug(string name) =>
        string.Concat(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant));

    // Mobile numbers in the style of the Ghanaian networks: 024-123-4567.
    private static string GhanaPhone(Rng rng) =>
        $"{rng.Pick(_phonePrefixes)}-{rng.Next(100, 1000)}-{rng.Next(1000, 10000)}";

    // Digital addresses look like GA-123-4567: a region code, then two groups of digits.
    private static string GpsAddress(Rng rng, Region region) =>
        $"{RegionCode(region)}-{rng.Next(100, 1000)}-{rng.Next(1000, 10000)}";

    private static string RegionCode(Region region) =>
        region switch
        {
            Region.GreaterAccra => "GA",
            Region.Ashanti => "AK",
            Region.Eastern => "ER",
            Region.Central => "CR",
            Region.Western => "WR",
            Region.Northern => "NR",
            Region.Volta => "VR",
            Region.Bono => "BR",
            Region.BonoEast => "BE",
            Region.WesternNorth => "WN",
            Region.UpperEast => "UE",
            Region.UpperWest => "UW",
            Region.Ahafo => "AH",
            Region.Oti => "OT",
            Region.Savannah => "SV",
            Region.NorthEast => "NE",
            _ => "GA",
        };

    // ── Reference data ────────────────────────────────────────────────────

    private static readonly string[] _stems =
    [
        "Cornerstone",
        "Redeemer",
        "Zoe",
        "Living Faith",
        "Harvest",
        "Bethel",
        "Shalom",
        "Trinity",
        "Rock of Ages",
        "New Covenant",
    ];

    private static readonly string[] _suffixes = ["Chapel", "Assembly", "Fellowship"];

    private static readonly string[] _phonePrefixes =
    [
        "024",
        "054",
        "055",
        "059",
        "020",
        "050",
        "027",
        "057",
        "026",
        "056",
    ];

    private static readonly IReadOnlyList<(Region Value, double Weight)> _weightedRegions =
    [
        (Region.GreaterAccra, 25.0),
        (Region.Ashanti, 20.0),
        (Region.Eastern, 8.0),
        (Region.Central, 8.0),
        (Region.Western, 7.0),
        (Region.Northern, 6.0),
        (Region.Volta, 6.0),
        (Region.Bono, 4.0),
        (Region.BonoEast, 3.0),
        (Region.WesternNorth, 3.0),
        (Region.UpperEast, 3.0),
        (Region.UpperWest, 2.5),
        (Region.Ahafo, 2.0),
        (Region.Oti, 1.5),
        (Region.Savannah, 1.5),
        (Region.NorthEast, 1.5),
    ];
}
