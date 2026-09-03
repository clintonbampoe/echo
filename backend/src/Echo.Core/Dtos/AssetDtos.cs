using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record AssetCreateDto : IPrimaryCreateDto
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

public record AssetUpdateDto : IPrimaryUpdateDto
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

public record AssetListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string CategoryName { get; init; }
    public required string Name { get; init; }
    public AssetStatus Status { get; init; }
    public decimal CurrentValue { get; init; }
}

public record AssetResponseDto : IPrimaryResponseDto
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
    public required int TotalAssets { get; init; }
    public required decimal TotalCurrentValue { get; init; }
    public required int UnderMaintenance { get; init; }
    public required decimal TotalDepreciation { get; init; }
}
