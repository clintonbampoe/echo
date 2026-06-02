
using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class Event : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity, ISearchableEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public Guid OrganizationId { get; init; }
    public Organization Organization { get; init; } = null!;
    public Guid OrganizerId { get; init; }
    public Member Organizer { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
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
