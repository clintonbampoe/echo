using Api.Core.Entities.Interfaces;

namespace Api.Core.Entities;

public class AttendanceType : IReferenceEntity, ISearchableEntity
{
    public int Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
