using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Asset
{
    public int AssetId { get; }
    public Guid UniqueId { get; }
    public string Name { get; }
    public string SerialNumber { get; }
    public int CategoryId { get; }
    public DateOnly PurchaseDate { get; }
    public decimal PurchaseCost { get; }
    public decimal CurrentValue { get; }
    public AssetStatus Status { get; }
    public string Description { get; }
}
