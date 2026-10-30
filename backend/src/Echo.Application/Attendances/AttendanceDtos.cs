using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;
using Echo.Shared.Utilities;

namespace Echo.Application.Attendances;

public record AttendanceCreateDto
{
    public Guid SessionId { get; init; }

    public Guid PersonId { get; init; }

    /// <summary>
    /// Optional. If omitted, the service uses the current UTC time.
    /// </summary>
    public DateTime? CheckedInAt { get; init; }
}

public record AttendanceUpdateDto
{
    public DateTime? CheckedInAt { get; init; }

    // PersonId and SessionId are immutable. Delete and recreate to change either.
    // PersonKindAtTime is immutable. It reflects what was true when the
    // attendance was recorded.
}

public record AttendanceResponseDto
{
    public Guid Id { get; init; }

    public Guid SessionId { get; init; }
    public DateOnly SessionDate { get; init; }
    public string? SessionName { get; init; }
    public int SessionLabelId { get; init; }
    public required string SessionLabelName { get; init; }

    public Guid PersonId { get; init; }
    public required string PersonName { get; init; }

    /// <summary>
    /// The person's role at the time this attendance was recorded.
    /// Not necessarily their current role — a visitor who later became a
    /// member still shows as Visitor for their pre-conversion attendances.
    /// </summary>
    public PersonKind PersonKind { get; init; }

    public DateTime CheckedInAt { get; init; }
    public Guid? RecordedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttendanceCursor
{
    public DateOnly SessionDate { get; init; }
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this week to today (by Session.Date).
/// No default window when SessionId or PersonId is set.
/// </summary>
public record AttendanceFilters
{
    public Guid? PersonId { get; init; }
    public Guid? SessionId { get; init; }
    public int? SessionLabelId { get; init; }
    public PersonKind? Kind { get; init; }

    /// <summary>Case-insensitive match anywhere in the person's name.</summary>
    public string? Name { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }

}

/// <summary>
/// Narrows the monthly summary. Months and To choose the months; the other filter chooses the
/// attendance.
/// </summary>
public record AttendanceMonthlyFilters
{
    /// <summary>Only attendance at sessions with this label (for example "Sunday Service").</summary>
    public int? SessionLabelId { get; init; }

    /// <summary>How many months to return, ending with the month of <see cref="To"/>. 12 when not given.</summary>
    [Range(1, MonthRange.MaxMonths)]
    public int? Months { get; init; }

    /// <summary>Any day in the last month to return. This month when not given.</summary>
    public DateOnly? To { get; init; }
}

/// <summary>The attendance of one month, split into members, visitors and first-time visitors.</summary>
public record AttendanceMonthlyDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalPresent { get; init; }
    public int MembersPresent { get; init; }
    public int VisitorsPresent { get; init; }
    public int FirstTimeVisitors { get; init; }
}

public record AttendanceSummaryDto
{
    public int TotalPresent { get; init; }
    public int MembersPresent { get; init; }
    public int VisitorsPresent { get; init; }
    public int FirstTimeVisitors { get; init; }

    /// <summary>How many sessions were held (completed) in the window, among the ones `sessionLabelId` picks.</summary>
    public int Sessions { get; init; }

    /// <summary><see cref="TotalPresent"/> over <see cref="Sessions"/>: how many people a session draws. Zero when none was held.</summary>
    public double AveragePerSession { get; init; }

    /// <summary>
    /// <see cref="TotalPresent"/> for the stretch before the window (as many days, ending the day before `from`).
    /// Zero unless `includePrevious` is true.
    /// </summary>
    public int PreviousTotalPresent { get; init; }
    public DateWindow? Window { get; init; }
}

public record BulkAttendanceCreateDto
{
    public Guid SessionId { get; init; }

    [Required, MinLength(1), MaxLength(500)]
    public List<Guid> PersonIds { get; init; } = [];

    /// <summary>Optional. Applied to every record created by the request. Defaults to the current UTC time.</summary>
    public DateTime? CheckedInAt { get; init; }
}

public enum BulkSkipReason
{
    AlreadyPresent,
    PersonNotFound,
}

public record BulkSkippedDto
{
    public Guid PersonId { get; init; }
    public BulkSkipReason Reason { get; init; }
}

public record BulkAttendanceResponseDto
{
    public List<AttendanceResponseDto> Created { get; init; } = [];
    public List<BulkSkippedDto> Skipped { get; init; } = [];
}
