using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Seeder.Core;
using Echo.Seeder.Fakers;

namespace Echo.Seeder.Phases;

/// <summary>
/// Creates congregations, admin users, and all lookup tables.
///
/// Why EF here instead of COPY?
///   These tables have int PKs with DB-generated sequences. After
///   SaveChangesAsync, EF populates those IDs back onto the entities. Later
///   phases use those IDs as FKs in their COPY statements. There's no safe way
///   to do this with raw COPY without a second round-trip to read the IDs back.
///   Volume is tiny (a handful of rows per congregation) so EF is fine.
///
/// What this produces (written into each CongregationSeed):
///   - Congregation, Admin, AttendanceTypes, AssetCategories,
///     ProjectCategories, TransactionCategories
/// </summary>
public sealed class SetupPhase(SeedFakers fakers) : ISeedPhase
{
    public string Name => "Setup";

    public async Task RunAsync(SeederContext ctx, CancellationToken ct)
    {
        var rng = ctx.RngFor(Name);
        var now = ctx.Now;

        // ── Congregations ─────────────────────────────────────────────────

        var big = MakeCongregation(fakers, rng, "Grace Chapel International", isBig: true, now);
        var congregations = new List<Congregation> { big };

        for (var i = 1; i <= ctx.Options.SmallCount; i++)
            congregations.Add(MakeCongregation(fakers, rng, SmallName(rng, i), isBig: false, now));

        await ctx.Db.Congregations.AddRangeAsync(congregations, ct);
        await ctx.Db.SaveChangesAsync(ct);
        Log.CreatedCongregations(ctx.Logger, congregations.Count, ctx.Options.SmallCount);

        // Create Admins
        var admins = congregations.Select(c => MakeAdmin(fakers, c.Id, now)).ToList();
        await ctx.Db.Users.AddRangeAsync(admins, ct);
        await ctx.Db.SaveChangesAsync(ct);
        Log.CreatedAdmins(ctx.Logger, admins.Count);

        // Lookup tables (int PKs, EF generates them)
        var attendanceTypes = new List<AttendanceType>();
        var assetCategories = new List<AssetCategory>();
        var projectCategories = new List<ProjectCategory>();
        var transactionCategories = new List<TransactionCategory>();

        foreach (var congregation in congregations)
        {
            foreach (var name in rng.PickSubset(_attendanceTypeNames, rng.Next(4, 8)))
                attendanceTypes.Add(
                    new AttendanceType
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CreatedAt = now,
                    }
                );

            foreach (var name in rng.PickSubset(_assetCategoryNames, rng.Next(4, 9)))
                assetCategories.Add(
                    new AssetCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CreatedAt = now,
                    }
                );

            foreach (var name in rng.PickSubset(_projectCategoryNames, rng.Next(3, 7)))
                projectCategories.Add(
                    new ProjectCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CreatedAt = now,
                    }
                );

            foreach (var (name, type) in rng.PickSubset(_transactionCategoryPairs, rng.Next(6, 12)))
                transactionCategories.Add(
                    new TransactionCategory
                    {
                        CongregationId = congregation.Id,
                        Name = name,
                        CategoryType = type,
                        CreatedAt = now,
                    }
                );
        }

        await ctx.Db.AttendanceTypes.AddRangeAsync(attendanceTypes, ct);
        await ctx.Db.AssetCategories.AddRangeAsync(assetCategories, ct);
        await ctx.Db.ProjectCategories.AddRangeAsync(projectCategories, ct);
        await ctx.Db.TransactionCategories.AddRangeAsync(transactionCategories, ct);
        await ctx.Db.SaveChangesAsync(ct);

        ctx.Logger.LogInformation(
            "Created lookup entities — attendance={A} assets={B} projects={P} transactions={T}.",
            attendanceTypes.Count,
            assetCategories.Count,
            projectCategories.Count,
            transactionCategories.Count
        );

        // ── Populate state ────────────────────────────────────────────────

        for (var i = 0; i < congregations.Count; i++)
        {
            var congregation = congregations[i];
            var seed = new CongregationSeed
            {
                Congregation = congregation,
                IsBig = congregation.Id == big.Id,
                Admin = admins[i],
            };

            seed.AttendanceTypes.AddRange(
                attendanceTypes.Where(t => t.CongregationId == congregation.Id)
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

    // ── Factories ─────────────────────────────────────────────────────────

    private static Congregation MakeCongregation(
        SeedFakers fakers,
        Rng rng,
        string name,
        bool isBig,
        DateTime now
    )
    {
        var slug = name.ToLowerInvariant().Replace(" ", "").Replace("'", "");
        return new Congregation
        {
            Id = Guid.NewGuid(),
            Name = name,
            OrgType = CongregationType.Church,
            PhoneNumber = fakers.Raw.Phone.PhoneNumber("0##-###-####"),
            EmailAddress = $"info@{slug}.org",
            Region = rng.Weighted(_weightedRegions),
            City = fakers.Raw.Address.City(),
            Town = fakers.Raw.Address.City(),
            GpsAddress = fakers.Raw.Address.Latitude().ToString("F4"),
            PostalAddress = isBig ? $"P.O. Box {rng.Next(1000, 9999)}" : null,
            WebsiteUrl = isBig ? $"https://www.{slug}.org" : null,
            CreatedAt = now,
        };
    }

    private static Domain.Users.User MakeAdmin(SeedFakers fakers, Guid congregationId, DateTime now)
    {
        var user = fakers.User.Generate();
        user.CongregationId = congregationId;
        user.CreatedAt = now;
        return user;
    }

    private static string SmallName(Rng rng, int index)
    {
        var stem = rng.Pick([
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
        ]);
        var suffix = rng.Pick(["Chapel", "Assembly", "Fellowship"]);
        return $"{stem} {suffix} {index}";
    }

    // ── Reference data ────────────────────────────────────────────────────

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

    private static readonly IReadOnlyList<string> _attendanceTypeNames =
    [
        "Sunday Service",
        "Midweek Service",
        "Prayer Meeting",
        "Bible Study",
        "Youth Service",
        "Communion Service",
        "Vigil",
        "Special Service",
        "Cell Group",
        "Morning Devotion",
    ];

    private static readonly IReadOnlyList<string> _assetCategoryNames =
    [
        "Equipment",
        "Furniture",
        "Vehicles",
        "Electronics",
        "Musical Instruments",
        "Sound System",
        "Computers",
        "Buildings",
        "Land",
        "Books & Media",
        "Generators",
        "Air Conditioning",
        "Tools",
    ];

    private static readonly IReadOnlyList<string> _projectCategoryNames =
    [
        "Construction",
        "Outreach",
        "Equipment",
        "Missions",
        "Education",
        "Welfare",
        "Youth",
        "Media",
        "Transportation",
        "Land Acquisition",
    ];

    private static readonly IReadOnlyList<(
        string Name,
        TransactionType Type
    )> _transactionCategoryPairs =
    [
        ("Offerings", TransactionType.Income),
        ("Tithes", TransactionType.Income),
        ("Donations", TransactionType.Income),
        ("Pledges", TransactionType.Income),
        ("Fundraising", TransactionType.Income),
        ("Utilities", TransactionType.Expense),
        ("Rent", TransactionType.Expense),
        ("Salaries", TransactionType.Expense),
        ("Maintenance", TransactionType.Expense),
        ("Transportation", TransactionType.Expense),
        ("Events", TransactionType.Expense),
        ("Outreach", TransactionType.Expense),
        ("Printing", TransactionType.Expense),
        ("Hospitality", TransactionType.Expense),
    ];
}

public static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Created {Count} congregation(s) (1 big, {Small} small)."
    )]
    public static partial void CreatedCongregations(this ILogger logger, int count, int small);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created {Count} admin user(s).")]
    public static partial void CreatedAdmins(this ILogger logger, int count);
}
