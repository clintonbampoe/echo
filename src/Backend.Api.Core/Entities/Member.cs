using System.ComponentModel.DataAnnotations;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities;

public class Member
{
    public int MemberId { get; }
    public Guid UniqueId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string OtherNames { get; }
    public string EmailAddress { get; }
    public string PhoneNumber { get; }
    public DateOnly DateOfBirth { get; }
    public DateOnly JoinedDate { get; }
    public Gender Gender { get; }
    public string ResidentialAddress { get; }
    public string City { get; }
    public string Hometown { get; }
    public Region Region { get; }
    public string GpsAddress { get; }
    public MaritalStatus MaritalStatus { get; }
    public string NextOfKin { get; }
    public string EmergencyContactName { get; }
    public string EmergencyContactPhoneNumber { get; }
    public MemberActivityStatus MemberActivityStatus { get; }
}
