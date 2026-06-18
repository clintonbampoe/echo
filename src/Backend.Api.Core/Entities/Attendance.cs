using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Attendance : IPrimaryEntity
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int AttendanceContextId {get; set; }
    public AttendanceContext AttendanceContext {get; set; } = null!;
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; } 
    public string? GuestName {get; set; }
    public DateOnly ForDate { get; set; }
    public AttendeeType AttendeeType { get; set; }
    public TimeOnly CheckInTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
