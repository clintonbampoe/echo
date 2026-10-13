using System.ComponentModel.DataAnnotations;
using Echo.Domain.Attendances;
using Echo.Shared.Utilities;

namespace Echo.Application.Attendances;

public record SessionCreateDto
{
    [Range(1, int.MaxValue)]
    public int SessionLabelId { get; init; }

    public DateOnly Date { get; init; }

    [StringLength(200)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Topic { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }

    public TimeOnly? StartsAt { get; init; }
    public TimeOnly? EndsAt { get; init; }
}

public record SessionUpdateDto
{
    [StringLength(200)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Topic { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }

    public TimeOnly? StartsAt { get; init; }
    public TimeOnly? EndsAt { get; init; }
    public SessionStatus? Status { get; init; }

    // Date and SessionTypeId are not editable. Changing either is a
    // different session — delete and recreate.
}

public record SessionResponseDto
{
    public Guid Id { get; init; }
    public int SessionLabelId { get; init; }
    public required string SessionLabelName { get; init; }
    public SessionCategory Category { get; init; }

    public DateOnly Date { get; init; }
    public string? Name { get; init; }
    public string? Topic { get; init; }
    public string? Notes { get; init; }
    public TimeOnly? StartsAt { get; init; }
    public TimeOnly? EndsAt { get; init; }
    public SessionStatus Status { get; init; }

    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }

    public int AttendanceCount { get; init; }
}

public record SessionCursor
{
    public DateOnly Date { get; init; }
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: the whole current week (Sunday to Saturday),
/// so sessions scheduled later this week are listed. Sessions scheduled beyond this week
/// need an explicit From/To.
/// </summary>
public record SessionFilters
{
    public int? SessionLabelId { get; init; }
    public SessionStatus? Status { get; init; }
    public string? Name { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record SessionSummaryDto
{
    public int TotalSessions { get; init; }
    public int CompletedSessions { get; init; }
    public int CancelledSessions { get; init; }
    public int TotalAttendance { get; init; }
    public double AverageAttendance { get; init; }
    public DateWindow? Window { get; init; }
}
