using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Asset : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? SerialNumber { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string? Description { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
