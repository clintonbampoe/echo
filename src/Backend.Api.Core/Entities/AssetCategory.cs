using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class AssetCategory : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
