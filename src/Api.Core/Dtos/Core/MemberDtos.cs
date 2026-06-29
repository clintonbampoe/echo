using Api.Core.Dtos.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos.Core;

public record MemberCreateDto(
    string FirstName,
    string LastName,
    string? OtherNames,
    string? EmailAddress,
    string PhoneNumber,
    DateOnly DateOfBirth,
    DateOnly? JoinedDate,
    Gender Gender,
    string ResidentialAddress,
    string City,
    string Hometown,
    Region Region,
    string? GpsAddress,
    MaritalStatus MaritalStatus,
    string NextOfKin,
    string EmergencyContactName,
    string EmergencyContactPhoneNumber,
    MemberActivityStatus MemberActivityStatus
) : IPrimaryCreateDto;

public record MemberUpdateDto(
    string FirstName,
    string LastName,
    string? OtherNames,
    string? EmailAddress,
    string PhoneNumber,
    DateOnly DateOfBirth,
    DateOnly? JoinedDate,
    Gender Gender,
    string ResidentialAddress,
    string City,
    string Hometown,
    Region Region,
    string? GpsAddress,
    MaritalStatus MaritalStatus,
    string NextOfKin,
    string EmergencyContactName,
    string EmergencyContactPhoneNumber,
    MemberActivityStatus MemberActivityStatus
) : IPrimaryUpdateDto;

public record MemberListResponseDto(
    Guid Id,
    string Name,
    string PhoneNumber,
    string? EmailAddress,
    Gender Gender,
    MemberActivityStatus MemberActivityStatus
) : IPrimaryListResponseDto;

public record MemberResponseDto(
    Guid Id,
    string Name,
    string FirstName,
    string LastName,
    string? OtherNames,
    string? EmailAddress,
    string PhoneNumber,
    DateOnly DateOfBirth,
    DateOnly? JoinedDate,
    Gender Gender,
    string ResidentialAddress,
    string City,
    string Hometown,
    Region Region,
    string? GpsAddress,
    MaritalStatus MaritalStatus,
    string NextOfKin,
    string EmergencyContactName,
    string EmergencyContactPhoneNumber,
    MemberActivityStatus MemberActivityStatus,
    DateTime CreatedAt
) : IPrimaryResponseDto;
