using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Organization : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; } = string.Empty;
    public DateOnly CreatedAt { get; init; }
    public DateTime? DeletedAt { get; set; }
}
