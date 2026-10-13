using System.ComponentModel.DataAnnotations;
using Echo.Shared.Utilities;
using Echo.Shared.Pagination;

namespace Echo.Application.Events;

public record EventCreateDto
{
    public Guid OrganizationId { get; init; }
    public Guid OrganizerId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    [StringLength(255)]
    public string? Location { get; init; }

    [Range(1, 100_000)]
    public int? Capacity { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record EventUpdateDto
{
    public Guid? OrganizationId { get; init; }
    public Guid? OrganizerId { get; init; }

    [StringLength(100)]
    public string? Name { get; init; }

    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }

    [StringLength(255)]
    public string? Location { get; init; }

    [Range(1, 100_000)]
    public int? Capacity { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record EventResponseDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public required string OrganizationName { get; init; }
    public Guid OrganizerId { get; init; }
    public required string OrganizerName { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public string? Location { get; init; }
    public int? Capacity { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public int RegistrationCount { get; init; }
}

public record EventSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public record EventCursor
{
    public DateOnly StartDate { get; init; }
    public Guid Id { get; init; }

    // The direction the page was listed in. A cursor only continues a list in the same direction.
    public SortDirection Order { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year (by StartDate).
/// </summary>
public record EventFilters
{
    public string? Name { get; init; }
    public Guid? OrganizationId { get; init; }
    public Guid? OrganizerId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on StartDate

    /// <summary>Asc (default) lists the oldest event first, Desc the newest. Ignored by the summary.</summary>
    public SortDirection Order { get; init; }
}

public record EventSummaryDto
{
    public int TotalEvents { get; init; }
    public int UpcomingEvents { get; init; }
    public int TotalRegistered { get; init; }
    public int TotalAttended { get; init; }
    public DateWindow? Window { get; init; }
}
