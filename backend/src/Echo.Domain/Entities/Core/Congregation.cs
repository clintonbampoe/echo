using Echo.Domain.Entities.Core.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Domain.Entities.Core;

public class Congregation : ISearchableEntity, ISoftDeletable
{
    public Guid Id { get; set; } = Guid.CreateVersion7(DateTime.UtcNow);

    public string Name { get; set; } = string.Empty;
    public ReligiousOrganizationType OrgType { get; set; }

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
