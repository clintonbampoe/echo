using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class EventAttendance : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Congregation Congregation { get; init; } = null!;
    public Guid MemberId { get; init; }
    public Member Member { get; init; } = null!;
    public Guid EventId { get; init; }
    public Event Event { get; init; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public TimeOnly CheckInTime { get; set; }
    public DateTime? DeletedAt { get; set; }
}
