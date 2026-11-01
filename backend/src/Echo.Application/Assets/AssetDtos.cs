using System.ComponentModel.DataAnnotations;
using Echo.Domain.Assets;
using Echo.Shared.Utilities;

namespace Echo.Application.Assets;

public record AssetCreateDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(100)]
    public string? SerialNumber { get; init; }

    public DateOnly? PurchaseDate { get; init; }

    [Range(0, 1_000_000)]
    public decimal PurchaseCost { get; init; }

    [Range(0, 1_000_000)]
    public decimal CurrentValue { get; init; }

    public AssetStatus Status { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record AssetUpdateDto
{
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(100)]
    public string? SerialNumber { get; init; }

    public DateOnly? PurchaseDate { get; init; }

    [Range(0, 1_000_000)]
    public decimal? PurchaseCost { get; init; }

    [Range(0, 1_000_000)]
    public decimal? CurrentValue { get; init; }

    public AssetStatus? Status { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record AssetResponseDto
{
    public Guid Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string Name { get; init; }
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }

    /// <summary>The day it was liquidated; none while the church still owns it.</summary>
    public DateOnly? LiquidatedOn { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AssetSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public record AssetCursor
{
    public string Name { get; init; } = string.Empty;
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year (by PurchaseDate).
/// </summary>
public record AssetFilters
{
    public AssetStatus? Status { get; init; }

    /// <summary>
    /// Whether liquidated (sold or scrapped) assets are included. Not set means the list shows
    /// them and the summary leaves them out, because its totals describe what the church owns now.
    /// A Status of Liquidated always shows them.
    /// </summary>
    public bool? IncludeLiquidated { get; init; }
    public int? CategoryId { get; init; }
    public string? Name { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record AssetSummaryDto
{
    public int TotalAssets { get; init; }
    public decimal TotalCurrentValue { get; init; }
    public decimal TotalPurchaseCost { get; init; }
    public decimal TotalDepreciation { get; init; }

    /// <summary>
    /// How old the assets are on average, in years, over those with a purchase date. Zero when none
    /// has one.
    /// </summary>
    public double AverageAgeYears { get; init; }
    public DateWindow? Window { get; init; }
}

/// <summary>What the assets of one category cost and are worth.</summary>
public record AssetCategoryTotalDto
{
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public int Count { get; init; }
    public decimal TotalPurchaseCost { get; init; }
    public decimal TotalCurrentValue { get; init; }
    public decimal TotalDepreciation { get; init; }
}

/// <summary>
/// Narrows the monthly chart. The dates choose the months, not the assets: a purchase date puts an
/// asset in its month, a liquidation date puts it in another.
/// </summary>
public record AssetMonthlyFilters
{
    public int? CategoryId { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>. 12 when not given.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int? Months { get; init; }

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

/// <summary>One month of assets bought and assets lost (liquidated). Costs are purchase costs.</summary>
public record AssetMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int AcquiredCount { get; init; }
    public decimal AcquiredCost { get; init; }
    public int LostCount { get; init; }
    public decimal LostCost { get; init; }
}
