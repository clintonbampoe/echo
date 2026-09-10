using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

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

public record AssetListResponseDto
{
    public Guid Id { get; init; }
    public required string CategoryName { get; init; }
    public required string Name { get; init; }
    public AssetStatus Status { get; init; }
    public decimal CurrentValue { get; init; }
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
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AssetSummaryDto
{
    public int TotalAssets { get; init; }
    public decimal TotalCurrentValue { get; init; }
    public int UnderMaintenance { get; init; }
    public decimal TotalDepreciation { get; init; }
}

public record AssetSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
