using System.Runtime.InteropServices.JavaScript;

namespace Backend.Api.Core.Entities;

public class EventRegistration
{
    public int MemberId { get; }
    public int EventId { get; }
    public Guid UniqueId { get; }
    public DateOnly RegistrationDate { get; }
}
