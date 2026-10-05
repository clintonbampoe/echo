using Echo.Domain.Congregations;
using Echo.Domain.Members;

namespace Echo.Domain.Attendances;

public class Attendance : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int AttendanceTypeId { get; set; }
    public AttendanceType AttendanceType { get; set; } = null!;

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateOnly Date { get; set; }
    public TimeOnly CheckInTime { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
