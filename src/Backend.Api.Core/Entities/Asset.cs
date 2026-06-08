using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Asset : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public Guid CategoryId { get; init; }
    public AssetCategory Category { get; init; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; } = string.Empty;
    public DateOnly? PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetStatus Status { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
