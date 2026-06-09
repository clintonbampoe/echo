using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record AssetCreateDto : ICreateDto<Asset>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public string? Description { get; init; }
}

public record AssetResponseDto : IResponseDto<Asset>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public string Category { get; init; } = string.Empty; // flattened from nav property
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string? Description { get; init; }
}

public record AssetListResponseDto : IListResponseDto<Asset>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public AssetStatus Status { get; init; }
    public decimal CurrentValue { get; init; }
}

public record AssetUpdateDto : IUpdateDto<Asset>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string? Description { get; init; }
}

public record AssetDeleteDto : ISoftDeleteDto<Asset>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
