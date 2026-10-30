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
    public int AttendanceCount { get; init; }
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
}

/// <summary>The list's filters: the shared ones, and which end of the calendar to start from.</summary>
public record EventListFilters : EventFilters
{
    /// <summary>Asc (default) lists the oldest event first, Desc the newest.</summary>
    public SortDirection? Order { get; init; }
}

/// <summary>One event with how many registered and how many came: a bar in a per-event chart.</summary>
public record EventSeriesDto
{
    public Guid EventId { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public int? Capacity { get; init; }
    public int RegistrationCount { get; init; }
    public int AttendanceCount { get; init; }
}

/// <summary>
/// Narrows the monthly chart. The dates choose the months an event starts in; the other filters
/// choose the events.
/// </summary>
public record EventMonthlyFilters
{
    public Guid? OrganizationId { get; init; }
    public Guid? OrganizerId { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>. 12 when not given.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int? Months { get; init; }

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

/// <summary>Filters for the per-event series: the shared event filters and how many events to return.</summary>
public record EventSeriesFilters : EventFilters
{
    /// <summary>How many events to return, the newest first. 12 when not given.</summary>
    [Range(1, 50)]
    public int? Limit { get; init; }
}

/// <summary>The events that started in one month, with their registrations and check-ins.</summary>
public record EventMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int EventCount { get; init; }
    public int RegistrationCount { get; init; }
    public int AttendanceCount { get; init; }
}

public record EventSummaryDto
{
    public int TotalEvents { get; init; }
    public int UpcomingEvents { get; init; }
    public int TotalRegistered { get; init; }
    public int TotalAttended { get; init; }
    public DateWindow? Window { get; init; }
}
