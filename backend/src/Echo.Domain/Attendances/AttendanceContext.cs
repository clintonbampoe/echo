using Echo.Domain.Congregations;

namespace Echo.Domain.Attendances;

public class AttendanceContext : ISearchable, ISoftDeletable
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public int AttendanceTypeId { get; set; }
    public AttendanceType AttendanceType { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
