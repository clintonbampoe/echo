using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class EventAttendance : ICongregationEntity, ISoftDeletableEntity
{
    public Guid MemberId { get; init; }
    public Member Member { get; init; } = null!;
    public Guid EventId { get; init; }
    public Event Event { get; init; } = null!;
    public TimeOnly CheckInTime { get; set; }
    public DateTime? DeletedAt { get; set; }
}
