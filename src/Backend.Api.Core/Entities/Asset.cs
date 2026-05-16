using System.ComponentModel.DataAnnotations;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Asset
{
    public int AssetId { get; init; }
    public Guid UniqueId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public decimal PurchaseCost { get; init; }
    public decimal CurrentValue { get; init; }
    public AssetStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
}
