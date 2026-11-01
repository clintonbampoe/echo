using System.ComponentModel.DataAnnotations;

namespace Echo.Application.Events;

public record EventAttendanceCreateDto
{
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceUpdateDto
{
    public TimeOnly? CheckInTime { get; init; }
}

public record EventAttendanceResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public Guid EventId { get; init; }
    public required string EventName { get; init; }
    public DateOnly EventStartDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// True when they came without registering. Only the list for one event fills it in (with
    /// <see cref="RegistrationId"/>); it is null on every other route.
    /// </summary>
    public bool? WalkIn { get; init; }

    /// <summary>Their registration for this event, when they had one. Only the list for one event fills it in.</summary>
    public Guid? RegistrationId { get; init; }
}

/// <summary>A check-in as another record points at it.</summary>
public record EventCheckInRefDto
{
    public Guid Id { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

/// <summary>Whether a person who checked in had registered first.</summary>
public enum CheckInKind
{
    /// <summary>They signed up before the event.</summary>
    Registered,

    /// <summary>They came without signing up.</summary>
    WalkIn,
}

/// <summary>Narrows the check-ins of one event.</summary>
public record EventAttendanceFilters
{
    /// <summary>Case-insensitive match anywhere in the member's name.</summary>
    public string? Name { get; init; }

    /// <summary>Only those who had registered, or only walk-ins.</summary>
    public CheckInKind? Kind { get; init; }

    /// <summary>
    /// Only these members (at most 50): to ask which of a handful of people have already checked in, without
    /// reading the whole list.
    /// </summary>
    [MaxLength(50)]
    public Guid[]? MemberIds { get; init; }
}

/// <summary>The numbers an event's page shows about who came. The sign-up count is on the event itself.</summary>
public record EventRosterSummaryDto
{
    /// <summary>Everyone who checked in.</summary>
    public int CheckedIn { get; init; }

    /// <summary>Of those, who came without registering.</summary>
    public int WalkIns { get; init; }

    /// <summary>Of those, who had registered (<see cref="CheckedIn"/> minus <see cref="WalkIns"/>).</summary>
    public int CameRegistered { get; init; }
}

public record EventAttendanceCursor
{
    public TimeOnly CheckInTime { get; init; }
    public Guid Id { get; init; }
}
