using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record EventAttendanceCreateDto(Guid MemberId, Guid EventId, TimeOnly CheckInTime)
    : IPrimaryCreateDto<EventAttendance>;

public record EventAttendanceUpdateDto(TimeOnly CheckInTime) : IPrimaryUpdateDto<EventAttendance>;

public record EventAttendanceListResponseDto(
    Guid Id,
    string MemberName,
    string EventName,
    TimeOnly CheckInTime
) : IPrimaryListResponseDto<EventAttendance>;

public record EventAttendanceResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid EventId,
    string EventName,
    TimeOnly CheckInTime,
    DateTime CreatedAt
) : IPrimaryResponseDto<EventAttendance>;
