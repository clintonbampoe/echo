namespace Backend.Api.Core.Entities;

public class EventAttendance : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}
