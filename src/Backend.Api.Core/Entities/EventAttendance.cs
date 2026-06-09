using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class EventAttendance : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public TimeOnly CheckInTime { get; set; }
    public DateTime? DeletedAt { get; set; }
}
