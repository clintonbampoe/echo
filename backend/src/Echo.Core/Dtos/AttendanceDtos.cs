using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record AttendanceCreateDto
{
    [Range(1, int.MaxValue)]
    public int AttendanceContextId { get; init; }

    public required Guid MemberId { get; init; }

    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record AttendanceUpdateDto
{
    [Range(1, int.MaxValue)]
    public int? AttendanceContextId { get; init; }

    public Guid? MemberId { get; init; }

    public AttendeeType? AttendeeType { get; init; }
    public DateOnly? ForDate { get; init; }
    public TimeOnly? CheckInTime { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record AttendanceResponseDto
{
    public Guid Id { get; init; }
    public int AttendanceContextId { get; init; }
    public required string AttendanceContextName { get; init; }
    public required string AttendanceTypeName { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttendanceCursor
{
    public DateOnly ForDate { get; init; }
    public Guid Id { get; init; }
}

public record AttendanceFilters
{
    public DateOnly? ForDate { get; init; }
    public int? AttendanceContextId { get; init; }
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; }
}
