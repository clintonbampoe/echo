using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record ProjectCreateDto(
    int CategoryId,
    Guid ManagerId,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description
) : IPrimaryCreateDto<Project>;

public record ProjectUpdateDto(
    int CategoryId,
    Guid ManagerId,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? Description
) : IPrimaryUpdateDto<Project>;

public record ProjectListResponseDto(
    Guid Id,
    string CategoryName,
    string ManagerName,
    string Name,
    decimal TargetAmount,
    ProjectStatus Status,
    DateOnly StartDate,
    DateOnly? EndDate
) : IPrimaryListResponseDto<Project>;

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
) : IPrimaryResponseDto<Project>;
