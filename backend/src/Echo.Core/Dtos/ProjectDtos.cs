using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record ProjectCreateDto : IPrimaryCreateDto
{
    public int CategoryId { get; init; }
    public Guid ManagerId { get; init; }
    public required string Name { get; init; }
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}

public record ProjectUpdateDto : IPrimaryUpdateDto
{
    public int CategoryId { get; init; }
    public Guid ManagerId { get; init; }
    public required string Name { get; init; }
    public decimal TargetAmount { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Description { get; init; }
}

public record ProjectListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
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
