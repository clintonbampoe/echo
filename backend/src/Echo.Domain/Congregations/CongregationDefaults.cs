using Echo.Domain.Attendances;
using Echo.Domain.Transactions;

namespace Echo.Domain.Congregations;

/// <summary>
/// Default lookup rows created for every new congregation.
/// <br/><br/>
/// These are the starting point, not a fixed set — congregations can
/// rename, add, or soft-delete any of them after creation. The lists
/// exist so a brand-new congregation is immediately usable without
/// forcing the first admin to set up a dozen categories before they can
/// record a single offering or attendance.
/// <br/><br/>
/// Both the registration flow and the seeder read from here. If you
/// change a list, both paths pick it up automatically.
/// </summary>
public static class CongregationDefaults
{
    /// <summary>
    /// Default session labels. Name is user-editable; category is fixed
    /// at creation and used for cross-congregation analytics.
    /// </summary>
    public static IReadOnlyList<(string Name, SessionCategory Category)> SessionLabels { get; } =
    [
        ("Service", SessionCategory.Service),
        ("Meeting", SessionCategory.Meeting),
        ("Bible Study", SessionCategory.Study),
        ("Prayer", SessionCategory.Prayer),
        ("Practice", SessionCategory.Practice),
        ("Outreach", SessionCategory.Outreach),
        ("Class", SessionCategory.Class),
        ("Special", SessionCategory.Special),
    ];

    public static IReadOnlyList<(
        string Name,
        TransactionType Type
    )> TransactionCategories { get; } =
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

    public static IReadOnlyList<string> ProjectCategoryNames { get; } =
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

    public static IReadOnlyList<string> AssetCategoryNames { get; } =
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
}
