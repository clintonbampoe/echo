using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Event : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public Guid OrganizationId { get; init; }
    public Organization Organization { get; init; } = null!;
    public Guid OrganizerId { get; init; }
    public Member Organizer { get; init; } = null!;
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
