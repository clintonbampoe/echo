
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Event : ICongregationEntity, ISoftDeletableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; } = string.Empty;
    public int? Capacity { get; init; }
    public string? Description { get; init; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
