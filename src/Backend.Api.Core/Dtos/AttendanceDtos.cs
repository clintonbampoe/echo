using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record AttendanceCreateDto(
    int AttendanceContextId,
    Guid? MemberId,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime,
    string? Description
) : IPrimaryCreateDto<Attendance>;

public record AttendanceUpdateDto(
    int AttendanceContextId,
    Guid? MemberId,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime,
    string? Description
) : IPrimaryUpdateDto<Attendance>;

public record AttendanceListResponseDto(
    Guid Id,
    string AttendanceContextName,
    string AttendanceTypeName,
    string? MemberName,
    string? GuestName,
    AttendeeType AttendeeType,
    DateOnly ForDate,
    TimeOnly CheckInTime
) : IPrimaryListResponseDto<Attendance>;

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
) : IPrimaryResponseDto<Attendance>;
