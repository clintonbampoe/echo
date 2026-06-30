using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record AttendanceCreateDto : IPrimaryCreateDto
{
    public int AttendanceContextId { get; init; }
    public Guid? MemberId { get; init; }
    public string? GuestName { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
}

public record AttendanceUpdateDto : IPrimaryUpdateDto
{
    public int AttendanceContextId { get; init; }
    public Guid? MemberId { get; init; }
    public string? GuestName { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
}

public record AttendanceListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string AttendanceContextName { get; init; }
    public required string AttendanceTypeName { get; init; }
    public string? MemberName { get; init; }
    public string? GuestName { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record AttendanceResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public int AttendanceContextId { get; init; }
    public required string AttendanceContextName { get; init; }
    public required string AttendanceTypeName { get; init; }
    public Guid? MemberId { get; init; }
    public string? MemberName { get; init; }
    public string? GuestName { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public DateOnly ForDate { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
