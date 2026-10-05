using System.ComponentModel.DataAnnotations;

namespace Echo.Application.Organizations;

public record OrganizationCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record OrganizationUpdateDto
{
    [StringLength(100)]
    public string? Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record OrganizationResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrganizationSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public record OrganizationFilters
{
    public string? Name { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on CreatedAt
}

public record OrganizationSummaryDto
{
    public int TotalOrganizations { get; init; }
    public int TotalMembers { get; init; }
    public double AverageMembersPerOrganization { get; init; }
    public string? LargestOrganization { get; init; }
}

public record OrganizationCursor
{
    public string Name { get; init; } = string.Empty;
    public Guid Id { get; init; }
}
