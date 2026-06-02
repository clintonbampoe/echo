using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Organization : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
