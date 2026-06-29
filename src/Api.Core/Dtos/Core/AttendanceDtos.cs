using Api.Core.Dtos.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos.Core;

public record AttendanceCreateDto(
    int AttendanceContextId,
    Guid? MemberId,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime,
    string? Description
) : IPrimaryCreateDto;

public record AttendanceUpdateDto(
    int AttendanceContextId,
    Guid? MemberId,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime,
    string? Description
) : IPrimaryUpdateDto;

public record AttendanceListResponseDto(
    Guid Id,
    string AttendanceContextName,
    string AttendanceTypeName,
    string? MemberName,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime
) : IPrimaryListResponseDto;

public record AttendanceResponseDto(
    Guid Id,
    int AttendanceContextId,
    string AttendanceContextName,
    string AttendanceTypeName,
    Guid? MemberId,
    string? MemberName,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
