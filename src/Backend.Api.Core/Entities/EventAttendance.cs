using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class EventAttendance : IPrimaryEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public TimeOnly CheckInTime { get; set; }
}
