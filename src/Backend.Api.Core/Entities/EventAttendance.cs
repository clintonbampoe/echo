namespace Backend.Api.Core.Entities;

public class EventAttendance
{
    public int MemberId { get; init; }
    public int EventId { get; init; }
    public Guid UniqueId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}
