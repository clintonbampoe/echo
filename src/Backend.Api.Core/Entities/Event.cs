
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Event : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity, ISearchableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public string? Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
