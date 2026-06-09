using Backend.Api.Core.Entities.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Member : ICongregationEntity, ISoftDeletableEntity, ISearchableEntity
{
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OtherNames { get; set; } = string.Empty;
    public string? EmailAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public DateOnly? JoinedDate { get; set; }
    public Gender Gender { get; set; }
    public string ResidentialAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Hometown { get; set; } = string.Empty;
    public Region Region { get; set; }
    public string? GpsAddress { get; set; } = string.Empty;
    public MaritalStatus MaritalStatus { get; set; }
    public string NextOfKin { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; set; } = string.Empty;
    public MemberActivityStatus MemberActivityStatus { get; set; }
    public DateTime? DeletedAt { get; set; }
}
