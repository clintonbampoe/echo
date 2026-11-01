using Echo.Domain.Congregations;

namespace Echo.Domain.Assets;

public class Asset : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public int CategoryId { get; set; }
    public AssetCategory Category { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; } = string.Empty;
    public DateOnly? PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal CurrentValue { get; set; }
    public AssetStatus Status { get; set; }

    /// <summary>The day the asset was liquidated. Set and cleared with the status.</summary>
    public DateOnly? LiquidatedOn { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
