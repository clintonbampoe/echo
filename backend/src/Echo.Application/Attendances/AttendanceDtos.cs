using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;

namespace Echo.Application.Attendances;

public record AttendanceCreateDto
{
    public Guid SessionId { get; init; }

    public Guid PersonId { get; init; }

    /// <summary>
    /// Optional. If omitted, the service sets it based on context:
    /// QR-scan check-ins get the current UTC time; admin-entered records
    /// leave it null unless the admin explicitly provides a value.
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

    public DateTime? CheckedInAt { get; init; }
    public Guid? RecordedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttendanceCursor
{
    public DateOnly SessionDate { get; init; }
    public Guid Id { get; init; }
}

public record AttendanceFilters
{
    public Guid? PersonId { get; init; }
    public Guid? SessionId { get; init; }
    public int? SessionLabelId { get; init; }
    public PersonKind? Kind { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record AttendanceSummaryDto
{
    public int TotalPresent { get; init; }
    public int MembersPresent { get; init; }
    public int VisitorsPresent { get; init; }
    public int FirstTimeVisitors { get; init; }
}
