namespace Backend.Api.Core.Entities;

public class EventAttendee
{
    public int MemberId { get; }
    public int EventId { get; }
    public Guid UniqueId { get; }
    public TimeOnly CheckInTime { get; }
}
