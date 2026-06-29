using Api.Core.Dtos.Core.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos.Core;

public record ProjectCreateDto(
    int CategoryId,
    Guid ManagerId,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description
) : IPrimaryCreateDto;

public record ProjectUpdateDto(
    int CategoryId,
    Guid ManagerId,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description
) : IPrimaryUpdateDto;

public record ProjectListResponseDto(
    Guid Id,
    string CategoryName,
    string ManagerName,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate
) : IPrimaryListResponseDto;

public record ProjectResponseDto(
    Guid Id,
    int CategoryId,
    string CategoryName,
    Guid ManagerId,
    string ManagerName,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
