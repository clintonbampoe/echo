namespace Backend.Api.Core.Entities;

public class AssetCategory : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
