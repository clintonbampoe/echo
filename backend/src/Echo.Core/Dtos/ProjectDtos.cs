using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record ProjectCreateDto : IPrimaryCreateDto
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

public record ProjectUpdateDto : IPrimaryUpdateDto
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

public record ProjectListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string CategoryName { get; init; }
    public required string ManagerName { get; init; }
    public required string Name { get; init; }
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}

public record ProjectResponseDto : IPrimaryResponseDto
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
}

public record ProjectSummaryDto
{
    public required int ActiveProjects { get; init; }
    public required decimal TotalRaised { get; init; }
    public required decimal TotalExpected { get; init; }
    public required int CompletedThisQuarter { get; init; }
}
