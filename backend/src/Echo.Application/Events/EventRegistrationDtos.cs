using System.ComponentModel.DataAnnotations;

namespace Echo.Application.Events;

public record EventRegistrationCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public DateOnly RegistrationDate { get; init; }
}

public record EventRegistrationUpdateDto
{
    public DateOnly? RegistrationDate { get; init; }
}

public record EventRegistrationResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public DateOnly EventStartDate { get; init; }
    public DateOnly RegistrationDate { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Their check-in at this event, when they came. Only the list for one event fills it in; it is null on
    /// every other route, where null does not mean they did not come.
    /// </summary>
    public EventCheckInRefDto? CheckIn { get; init; }
}

/// <summary>Whether a registered person came.</summary>
public enum RosterStatus
{
    /// <summary>They checked in.</summary>
    Came,

    /// <summary>They did not check in (a no-show once the event is over).</summary>
    Missing,
}

/// <summary>Narrows the registrations of one event.</summary>
public record EventRegistrationFilters
{
    /// <summary>Case-insensitive match anywhere in the member's name.</summary>
    public string? Name { get; init; }

    /// <summary>Only those who came, or only those who did not.</summary>
    public RosterStatus? Status { get; init; }

    /// <summary>
    /// Only these members (at most 50): to ask which of a handful of people are already registered, without
    /// reading the whole list.
    /// </summary>
    [MaxLength(50)]
    public Guid[]? MemberIds { get; init; }
}

public record EventRegistrationCursor
{
    public DateOnly RegistrationDate { get; init; }
    public Guid Id { get; init; }
}
