using Echo.Domain.Congregations;

namespace Echo.Domain.Members;

public class Member : ISoftDeletable
{
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public Guid CongregationId { get; set; }

    public DateOnly DateOfBirth { get; set; }
    public DateOnly? JoinedDate { get; set; }

    /// <summary>The day the member was baptised, if they were.</summary>
    public DateOnly? BaptisedDate { get; set; }
    public Gender Gender { get; set; }
    public string? ResidentialAddress { get; set; }
    public string? City { get; set; }
    public string? Hometown { get; set; }
    public Region? Region { get; set; }
    public string? GpsAddress { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? NextOfKin { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhoneNumber { get; set; }
    public MemberStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
