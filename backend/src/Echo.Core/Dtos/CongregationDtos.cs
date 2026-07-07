using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record CongregationCreateDto
{
    public string Name { get; init; } = string.Empty;
    public ReligiousOrganizationType OrgType { get; init; }

    public string PhoneNumber { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string? PostalAddress { get; init; }
    public string? WebsiteUrl { get; init; }

    public Region Region { get; init; }
    public string City { get; init; } = string.Empty;
    public string Town { get; init; } = string.Empty;
    public string GpsAddress { get; init; } = string.Empty;
}

public record CongregationUpdateDto
{
    public string? Name { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; } = string.Empty;
    public string? EmailAddress { get; init; } = string.Empty;
    public string? PostalAddress { get; init; }
    public string? WebsiteUrl { get; init; }

    public Region? Region { get; init; }
    public string? City { get; init; } = string.Empty;
    public string? Town { get; init; } = string.Empty;
    public string? GpsAddress { get; init; } = string.Empty;
}

public record CongregationResponseDto
{
    public Guid Id { get; set; }

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
}
