using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Member : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public string Name { get; } = string.Empty;
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
    public MemberStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
