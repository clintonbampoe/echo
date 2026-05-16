using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class AttendanceRecord
{
    public int AttendanceId { get; }
    public Guid UniqueId { get; }
    public int MemberId { get; }
    public DateOnly ForDate { get; }
    public ChurchServiceType ChurchServiceType { get; }
    public AttendeeType AttendeeType { get; }
    public TimeOnly CheckInTime { get; }
}
