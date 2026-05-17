namespace Backend.Api.Core.Entities;

public class EventRegistration
{
    public int MemberId { get; init; }
    public int EventId { get; init; }
    public Guid UniqueId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}
