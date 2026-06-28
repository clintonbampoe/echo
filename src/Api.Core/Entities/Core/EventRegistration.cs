using Api.Core.Entities.Core.Interfaces;

namespace Api.Core.Entities.Core;

public class EventRegistration : IPrimaryEntity
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
    public DateOnly RegistrationDate { get; set; }
}
