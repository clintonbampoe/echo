
namespace Backend.Api.Core.Entities;

public class Event
{
    public int EventId { get; init; }
    public Guid UniqueId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; } = string.Empty;
    public int OrganizationId { get; init; }
    public int OrganizerId { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; } = string.Empty;
}
