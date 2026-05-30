namespace Backend.Api.Core.Entities;

public class EventRegistration : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}
