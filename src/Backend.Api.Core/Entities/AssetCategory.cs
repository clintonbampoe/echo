using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class AssetCategory : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
