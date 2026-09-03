using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record CongregationCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    public ReligiousOrganizationType OrgType { get; init; }

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string EmailAddress { get; init; } = string.Empty;

    [StringLength(255)]
    public string? PostalAddress { get; init; }

    [Url, StringLength(500)]
    public string? WebsiteUrl { get; init; }

    public Region Region { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string City { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Town { get; init; } = string.Empty;

    [Required, StringLength(255, MinimumLength = 1)]
    public string GpsAddress { get; init; } = string.Empty;
}

public record CongregationUpdateDto
{
    [StringLength(100)]
    public string? Name { get; init; } = string.Empty;

    [Phone, StringLength(20)]
    public string? PhoneNumber { get; init; } = string.Empty;

    [EmailAddress, StringLength(255)]
    public string? EmailAddress { get; init; } = string.Empty;

    [StringLength(255)]
    public string? PostalAddress { get; init; }

    [Url, StringLength(500)]
    public string? WebsiteUrl { get; init; }

    public Region? Region { get; init; }

    [StringLength(100)]
    public string? City { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Town { get; init; } = string.Empty;

    [StringLength(255)]
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
