using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record ProjectCreateDto : ICreateDto<Project>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ManagerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}

public record ProjectResponseDto : IResponseDto<Project>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public string Category { get; init; } = string.Empty;
    public Guid ManagerId { get; init; }
    public string Manager { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}

public record ProjectListResponseDto : IListResponseDto<Project>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Manager { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
}

public record ProjectUpdateDto : IUpdateDto<Project>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid ManagerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}

public record ProjectDeleteDto : ISoftDeleteDto<Project>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
