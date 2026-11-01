using Echo.Domain.Congregations;

namespace Echo.Domain.Assets;

public class AssetCategory : ISearchable, ISoftDeletable
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Which icon the category is shown with. A key the client knows; nothing is validated.</summary>
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
