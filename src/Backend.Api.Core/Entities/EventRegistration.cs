using Backend.Api.Core.Entities.Interfaces;

namespace Backend.Api.Core.Entities;

public class EventRegistration : ICongregationEntity, ISoftDeletableEntity, IDateTrackedEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateOnly RegistrationDate { get; set; }
    public DateTime? DeletedAt { get; set; }
}
