using Backend.Api.Core.Dtos.Interfaces;

namespace Backend.Api.Core.Dtos;

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
