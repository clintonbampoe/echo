using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record OrganizationCreateDto : IPrimaryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record OrganizationUpdateDto : IPrimaryUpdateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record OrganizationListResponseDto
    : IPrimaryListResponseDto,
        Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record OrganizationResponseDto : IPrimaryResponseDto
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
