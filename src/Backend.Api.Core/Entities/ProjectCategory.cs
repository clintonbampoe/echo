using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class ProjectCategory : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity, ISearchableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
