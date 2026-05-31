using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class AssetCategory : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity, ISearchableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
