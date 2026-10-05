using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Member : ISoftDeletable
{
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public Guid CongregationId { get; set; }
    public Congregation Congregation { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }
    public DateOnly? JoinedDate { get; set; }
    public Gender Gender { get; set; }
    public string ResidentialAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Hometown { get; set; } = string.Empty;
    public Region Region { get; set; }
    public string? GpsAddress { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    public string NextOfKin { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhoneNumber { get; set; } = string.Empty;
    public MemberStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
