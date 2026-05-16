namespace Backend.Api.Core.Entities;

public class AssetCategory
{
    public int CategoryId { get; init; }
    public Guid UniqueId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
