using Echo.Domain.Congregations;
using Echo.Domain.Members;

namespace Echo.Domain.Events;

public class EventAttendance : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public TimeOnly CheckInTime { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
