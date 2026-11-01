using System.ComponentModel.DataAnnotations;
using Echo.Domain.Projects;
using Echo.Shared.Utilities;

namespace Echo.Application.Projects;

public record ProjectCreateDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public Guid ManagerId { get; init; }

    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal TargetAmount { get; init; }

    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record ProjectUpdateDto
{
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    public Guid? ManagerId { get; init; }

    [StringLength(100)]
    public string? Name { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal? TargetAmount { get; init; }

    public ProjectStatus? Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record ProjectResponseDto
{
    public Guid Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public Guid ManagerId { get; init; }
    public required string ManagerName { get; init; }
    public required string Name { get; init; }
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal RaisedAmount { get; init; }

    /// <summary>How many contributions the project has had in all, whatever the list is filtered by.</summary>
    public int TotalContributions { get; init; }
}

public record ProjectSearchResultDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}

public record ProjectCursor
{
    public DateOnly StartDate { get; init; }
    public Guid Id { get; init; }
}

/// <summary>
/// Default window when From/To are omitted: this year. A project is in the window when it
/// started on or before To and has not ended before From (open-ended projects stay visible).
/// </summary>
public record ProjectFilters
{
    public string? Name { get; init; }
    public ProjectStatus? Status { get; init; }
    public int? CategoryId { get; init; }
    public Guid? ManagerId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public record ProjectSummaryDto
{
    public int TotalProjects { get; init; }
    public decimal TotalRaised { get; init; }
    public decimal TotalTarget { get; init; }
    public int AtRiskCount { get; init; }

    /// <summary>
    /// Money given inside the window, to projects matching the filters whatever their dates.
    /// </summary>
    public decimal RaisedInWindow { get; init; }

    /// <summary>
    /// The same for the window before: as many days, ending the day before `from`. Compare the two
    /// to show how giving has changed.
    /// </summary>
    public decimal PreviousRaised { get; init; }
    public DateWindow? Window { get; init; }
}

/// <summary>How the projects of one category are doing.</summary>
public record ProjectCategoryTotalDto
{
    /// <summary>The category's id; none for the "Other" row, which stands for several categories.</summary>
    public int? CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public int ProjectCount { get; init; }
    public decimal TotalTarget { get; init; }
    public decimal TotalRaised { get; init; }
}
