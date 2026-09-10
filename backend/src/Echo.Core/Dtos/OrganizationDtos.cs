using System.ComponentModel.DataAnnotations;

namespace Echo.Core.Dtos;

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

public record OrganizationListResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record OrganizationResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrganizationSummaryDto
{
    public required int TotalOrganizations { get; init; }
    public required int TotalOrganizationMembers { get; init; }
    public required int NewOrganizationsThisMonth { get; init; }
    public required decimal AverageMembersPerOrganization { get; init; }
}

public record OrganizationSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
