using Echo.Domain.Congregations;
using Echo.Domain.Members;
using Echo.Domain.Organizations;

namespace Echo.Domain.Events;

public class Event : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid OrganizerId { get; set; }
    public Member Organizer { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Location { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
