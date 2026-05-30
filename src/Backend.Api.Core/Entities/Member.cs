using System.ComponentModel.DataAnnotations;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Member : ICongregationEntity
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? OtherNames { get; init; } = string.Empty;
    public string? EmailAddress { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public DateOnly? JoinedDate { get; init; }
    public Gender Gender { get; init; }
    public string ResidentialAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Hometown { get; init; } = string.Empty;
    public Region Region { get; init; }
    public string? GpsAddress { get; init; } = string.Empty;
    public MaritalStatus MaritalStatus { get; init; }
    public string NextOfKin { get; init; } = string.Empty;
    public string EmergencyContactName { get; init; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; init; } = string.Empty;
    public MemberActivityStatus MemberActivityStatus { get; init; }
}
