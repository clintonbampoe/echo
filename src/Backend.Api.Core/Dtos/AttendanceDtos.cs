using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record AttendanceCreateDto : ICreateDto<AttendanceRecord>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
}

public record AttendanceResponseDto : IResponseDto<AttendanceRecord>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public string Member { get; init; } = string.Empty;
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
}

public record AttendanceListResponseDto : IListResponseDto<AttendanceRecord>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Member { get; init; } = string.Empty;
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record AttendanceUpdateDto : IUpdateDto<AttendanceRecord>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public DateOnly ForDate { get; init; }
    public ChurchServiceType ChurchServiceType { get; init; }
    public AttendeeType AttendeeType { get; init; }
    public TimeOnly CheckInTime { get; init; }
    public string? Description { get; init; }
}

public record AttendanceDeleteDto : ISoftDeleteDto<AttendanceRecord>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
