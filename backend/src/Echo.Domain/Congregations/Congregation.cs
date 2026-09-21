using Echo.Domain.Members;

namespace Echo.Domain.Congregations;

public class Congregation : ISearchable, ISoftDeletable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public CongregationType OrgType { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? PostalAddress { get; set; }
    public string? WebsiteUrl { get; set; }

    public Region Region { get; set; }
    public string City { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string GpsAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
