using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record EventAttendanceCreateDto(Guid MemberId, Guid EventId, TimeOnly CheckInTime)
    : IPrimaryCreateDto;

public record EventAttendanceUpdateDto(TimeOnly CheckInTime) : IPrimaryUpdateDto;

public record EventAttendanceListResponseDto(
    Guid Id,
    string MemberName,
    string EventName,
    TimeOnly CheckInTime
) : IPrimaryListResponseDto;

public record EventAttendanceResponseDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid EventId,
    string EventName,
    TimeOnly CheckInTime,
    DateTime CreatedAt
) : IPrimaryResponseDto;
