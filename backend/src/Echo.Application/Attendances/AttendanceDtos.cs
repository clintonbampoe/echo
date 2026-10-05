using System.ComponentModel.DataAnnotations;
using Echo.Domain.Members;

namespace Echo.Application.Attendances;

public record AttendanceCreateDto
{
    [Range(1, int.MaxValue)]
    public int AttendanceTypeId { get; init; }

    public Guid PersonId { get; init; }

    public DateOnly Date { get; init; }
    public TimeOnly CheckInTime { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }
}

public record AttendanceUpdateDto
{
    public TimeOnly? CheckInTime { get; init; }

    [StringLength(2000)]
    public string? Notes { get; init; }
}

public record AttendanceResponseDto
{
    public Guid Id { get; init; }
    public int AttendanceTypeId { get; init; }
    public required string AttendanceTypeName { get; init; }
    public Guid PersonId { get; init; }
    public required string PersonName { get; init; }
    public PersonKind PersonKind { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttendanceCursor
{
    public DateOnly Date { get; init; }
    public Guid Id { get; init; }
}

public record AttendanceFilters
{
    public int? AttendanceTypeId { get; init; }
    public PersonKind? Kind { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on Date
}

public record AttendanceSummaryDto
{
    public int TotalPresent { get; init; }
    public int MembersPresent { get; init; }
    public int VisitorsPresent { get; init; }
    public int FirstTimeVisitors { get; init; }
}
