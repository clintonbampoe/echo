using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record MemberCreateDto : ICreateDto<Member>
{
    public Guid CongregationId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? OtherNames { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? EmailAddress { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }
    public string ResidentialAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Hometown { get; init; } = string.Empty;
    public Region Region { get; init; }
    public string? GpsAddress { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string NextOfKin { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; init; } = string.Empty;
}

public record MemberResponseDto : IResponseDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? OtherNames { get; init; }
    public string? EmailAddress { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }
    public string ResidentialAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Hometown { get; init; } = string.Empty;
    public Region Region { get; init; }
    public string? GpsAddress { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string NextOfKin { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; init; } = string.Empty;
    public MemberActivityStatus MemberActivityStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MemberListResponseDto : IListResponseDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public MemberActivityStatus MemberActivityStatus { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? EmailAddress { get; init; }
}

public record MemberUpdateDto : IUpdateDto<Member>
{
    public Guid CongregationId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? OtherNames { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? EmailAddress { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }
    public string ResidentialAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Hometown { get; init; } = string.Empty;
    public Region Region { get; init; }
    public string? GpsAddress { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string NextOfKin { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; init; } = string.Empty;
    public MemberActivityStatus MemberActivityStatus { get; init; }
}

public record MemberDeleteDto : ISoftDeleteDto<Member>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
