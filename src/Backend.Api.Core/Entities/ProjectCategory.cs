using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class ProjectCategory : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
