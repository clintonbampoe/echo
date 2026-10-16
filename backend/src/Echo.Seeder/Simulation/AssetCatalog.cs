namespace Echo.Seeder.Simulation;

/// <summary>How an asset's value changes with age.</summary>
public enum ValueTrend
{
    /// <summary>Loses value over its useful life, down to a tenth of the cost.</summary>
    Depreciates,

    /// <summary>Gains value each year, like land.</summary>
    Appreciates,
}

/// <summary>
/// One kind of asset a congregation owns.
/// </summary>
/// <param name="Category">The asset category it belongs to.</param>
/// <param name="Name">What it is called. A number is added when there are several.</param>
/// <param name="Cost">What one costs new today, in cedis.</param>
/// <param name="LifeYears">How many years it is useful for. Buildings and land last far longer.</param>
/// <param name="Trend">Whether it loses or gains value.</param>
/// <param name="Serial">
/// The prefix of its serial number, "PLATE" for a vehicle registration, or null
/// when it has none (a chair, a building).
/// </param>
/// <param name="MinBig">The fewest the big congregation owns.</param>
/// <param name="MaxBig">The most the big congregation owns.</param>
/// <param name="MinSmall">The fewest a typical small congregation owns.</param>
/// <param name="MaxSmall">The most a typical small congregation owns.</param>
public sealed record AssetKind(
    string Category,
    string Name,
    double Cost,
    int LifeYears,
    ValueTrend Trend,
    string? Serial,
    int MinBig,
    int MaxBig,
    int MinSmall,
    int MaxSmall
);

public static class AssetCatalog
{
    private const ValueTrend _down = ValueTrend.Depreciates;
    private const ValueTrend _up = ValueTrend.Appreciates;

    public static IReadOnlyList<AssetKind> All { get; } =
    [
        new("Sound System", "Mixing Console", 6_000, 8, _down, "MX", 1, 2, 0, 1),
        new("Sound System", "Speaker Cabinet", 2_500, 8, _down, "SP", 4, 8, 2, 4),
        new("Sound System", "Wireless Microphone Set", 1_800, 5, _down, "WM", 6, 12, 2, 4),
        new("Sound System", "Power Amplifier", 3_500, 8, _down, "PA", 2, 4, 1, 2),
        new("Sound System", "Stage Monitor", 1_500, 8, _down, "MN", 4, 8, 0, 2),
        new("Musical Instruments", "Keyboard", 7_000, 10, _down, "KB", 2, 4, 1, 2),
        new("Musical Instruments", "Drum Kit", 5_500, 10, _down, "DK", 1, 2, 1, 1),
        new("Musical Instruments", "Bass Guitar", 3_000, 12, _down, "BG", 1, 2, 0, 1),
        new("Musical Instruments", "Electric Guitar", 2_800, 12, _down, "EG", 1, 3, 0, 1),
        new("Musical Instruments", "Conga Drums", 1_200, 10, _down, null, 1, 2, 0, 1),
        new("Furniture", "Pulpit", 4_500, 20, _down, null, 1, 2, 1, 1),
        new("Furniture", "Plastic Chairs (Set of 50)", 2_500, 6, _down, null, 20, 60, 3, 10),
        new("Furniture", "Pews (Set of 10)", 12_000, 25, _down, null, 10, 30, 0, 5),
        new("Furniture", "Choir Stand", 3_000, 15, _down, null, 2, 4, 0, 2),
        new("Furniture", "Altar Table", 2_000, 20, _down, null, 1, 2, 1, 1),
        new("Furniture", "Offering Table", 800, 15, _down, null, 2, 4, 1, 2),
        new("Furniture", "Office Desk", 1_500, 12, _down, null, 4, 8, 1, 2),
        new("Furniture", "Filing Cabinet", 900, 15, _down, null, 3, 6, 1, 2),
        new("Electronics", "Projector", 4_500, 6, _down, "PJ", 3, 6, 1, 1),
        new("Electronics", "Projector Screen", 1_200, 8, _down, null, 3, 6, 1, 1),
        new("Electronics", "LED Television", 3_200, 8, _down, "TV", 4, 10, 0, 1),
        new("Electronics", "Digital Camera", 5_500, 6, _down, "CM", 2, 4, 0, 1),
        new("Electronics", "Streaming Encoder", 6_000, 6, _down, "EN", 1, 2, 0, 1),
        new("Computers", "Desktop Computer", 4_500, 6, _down, "DT", 6, 12, 1, 2),
        new("Computers", "Laptop", 6_000, 5, _down, "LT", 5, 10, 1, 2),
        new("Computers", "Laser Printer", 2_500, 6, _down, "PR", 3, 6, 1, 1),
        new("Computers", "Photocopier", 9_000, 8, _down, "PC", 1, 2, 0, 1),
        new("Vehicles", "Coaster Bus", 280_000, 12, _down, "PLATE", 1, 3, 0, 1),
        new("Vehicles", "Hiace Van", 160_000, 12, _down, "PLATE", 1, 3, 0, 1),
        new("Vehicles", "Pickup Truck", 150_000, 12, _down, "PLATE", 0, 2, 0, 0),
        new("Generators", "Diesel Generator", 45_000, 15, _down, "GN", 2, 3, 1, 1),
        new("Generators", "Petrol Generator", 6_000, 8, _down, "GN", 1, 3, 0, 2),
        new("Air Conditioning", "Split Air Conditioner", 4_500, 10, _down, "AC", 12, 30, 1, 5),
        new("Air Conditioning", "Standing Air Conditioner", 9_000, 10, _down, "AC", 2, 6, 0, 2),
        new("Buildings", "Main Sanctuary", 1_500_000, 50, _down, null, 1, 1, 0, 1),
        new("Buildings", "Church Annex", 600_000, 40, _down, null, 1, 1, 0, 0),
        new("Buildings", "Mission House", 450_000, 40, _down, null, 0, 1, 0, 0),
        new("Land", "Church Land", 400_000, 0, _up, null, 1, 1, 0, 1),
        new("Land", "Plot for Expansion", 250_000, 0, _up, null, 0, 2, 0, 0),
        new("Books & Media", "Pew Bible Set", 3_500, 10, _down, null, 3, 6, 1, 3),
        new("Books & Media", "Hymn Book Set", 1_800, 10, _down, null, 2, 4, 1, 2),
        new("Books & Media", "Church Library Collection", 6_000, 15, _down, null, 1, 1, 0, 0),
        new("Equipment", "Folding Canopy", 1_800, 6, _down, null, 4, 10, 1, 3),
        new("Equipment", "Standing Fans (Set of 10)", 2_500, 6, _down, null, 4, 10, 1, 3),
        new("Equipment", "Water Dispenser", 900, 6, _down, null, 2, 4, 0, 1),
        new("Equipment", "Horn Speakers for Outreach", 2_000, 8, _down, null, 1, 3, 0, 1),
        new("Tools", "Lawn Mower", 2_200, 8, _down, "LM", 1, 2, 0, 1),
        new("Tools", "Tool Kit", 600, 10, _down, null, 1, 2, 0, 1),
        new("Tools", "Ladder", 450, 10, _down, null, 1, 3, 0, 1),
    ];
}
