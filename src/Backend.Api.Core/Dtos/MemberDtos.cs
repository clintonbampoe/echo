using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

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
) : IPrimaryCreateDto<Member>;

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
) : IPrimaryUpdateDto<Member>;

public record MemberListResponseDto(
    Guid Id,
    string Name,
    string PhoneNumber,
    string? EmailAddress,
    Gender Gender,
    MemberActivityStatus MemberActivityStatus
) : IPrimaryListResponseDto<Member>;

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
) : IPrimaryResponseDto<Member>;
