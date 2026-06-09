using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record EventAttendanceCreateDto : ICreateDto<EventAttendance>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceResponseDto : IResponseDto<EventAttendance>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public string Member { get; init; } = string.Empty;
    public Guid EventId { get; init; }
    public string Event { get; init; } = string.Empty;
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceListResponseDto : IListResponseDto<EventAttendance>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Member { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceUpdateDto : IUpdateDto<EventAttendance>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public Guid EventId { get; init; }
    public TimeOnly CheckInTime { get; init; }
}

public record EventAttendanceDeleteDto : ISoftDeleteDto<EventAttendance>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
