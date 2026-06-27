using Api.Core.Entities.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Entities;

public class Asset : IPrimaryEntity, ISearchableEntity
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int CategoryId { get; set; }
    public AssetCategory Category { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; } = string.Empty;
    public DateOnly? PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetStatus Status { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
