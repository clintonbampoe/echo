using Echo.Domain.Congregations;
using Echo.Domain.Members;

namespace Echo.Domain.Attendances;

public class Attendance : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int AttendanceContextId { get; set; }
    public AttendanceContext AttendanceContext { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateOnly ForDate { get; set; }
    public AttendeeType AttendeeType { get; set; }
    public TimeOnly CheckInTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
    public string? Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
