using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record AssetCreateDto : IPrimaryCreateDto
{
    public int CategoryId { get; init; }
    public required string Name { get; init; }
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string? Description { get; init; }
}

public record AssetUpdateDto : IPrimaryUpdateDto
{
    public int CategoryId { get; init; }
    public required string Name { get; init; }
    public string? SerialNumber { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string? Description { get; init; }
}

public record AssetListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
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
