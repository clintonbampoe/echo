using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class AttendanceRecord
{
    public int AttendanceId { get; init; }
    public Guid UniqueId { get; init; }
    public int MemberId { get; init; }
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
}
