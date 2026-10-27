using System.ComponentModel.DataAnnotations;
using Echo.Domain.Organizations;
using Echo.Shared.Utilities;

namespace Echo.Application.Organizations;

public record OrganizationCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public OrganizationType Kind { get; init; } = OrganizationType.Other;
    public DayOfWeek? MeetingDay { get; init; }
}

public record OrganizationUpdateDto
{
    [StringLength(100)]
    public string? Name { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public OrganizationType? Kind { get; init; }
    public DayOfWeek? MeetingDay { get; init; }
}

public record OrganizationResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public OrganizationType Kind { get; init; }
    public DayOfWeek? MeetingDay { get; init; }
    public DateTime CreatedAt { get; init; }
    public int MemberCount { get; init; }

    /// <summary>The name of the longest-serving leader; none while the organization has no leader.</summary>
    public string? LeaderName { get; init; }

    /// <summary>
    /// The members' average age in whole years. Only worked out for a single organization, not in the
    /// list; none while it has no members.
    /// </summary>
    public double? AverageAge { get; init; }
}

public record OrganizationSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year (by CreatedAt).
/// </summary>
public record OrganizationFilters
{
    public string? Name { get; init; }
    public OrganizationType? Kind { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; } // on CreatedAt
}

public record OrganizationSummaryDto
{
    public int TotalOrganizations { get; init; }
    public int TotalMembers { get; init; }
    public double AverageMembersPerOrganization { get; init; }
    public string? LargestOrganization { get; init; }

    /// <summary>How many members the largest organization has. Zero when there are none.</summary>
    public int LargestOrganizationMemberCount { get; init; }
    public DateWindow? Window { get; init; }
}

public record OrganizationCursor
{
    public string Name { get; init; } = string.Empty;
    public Guid Id { get; init; }
}
