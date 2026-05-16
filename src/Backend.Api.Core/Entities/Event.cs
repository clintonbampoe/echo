namespace Backend.Api.Core.Entities;

public class Event
{
    public int EventId { get; }
    public Guid UniqueId { get; }
    public string Title { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }
    public string Location { get; }
    public int OrganizationId { get; }
    public int OrganizerId { get; }
    public int Capacity { get; }
    public string Description { get; }
}
