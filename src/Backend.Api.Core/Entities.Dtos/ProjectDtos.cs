using Backend.Api.Core.Entities.Dtos.Interfaces;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Entities.Dtos;

public record ProjectCreateDto
    (
        Guid Id,
        Guid CongregationId,
        Guid CategoryId,
        Guid ManagerId,
        string Name,
        decimal TargetAmount,
        ProjectStatus Status,
        DateOnly StartDate,
        DateOnly? EndDate,
        string? Description
    ) : ICreateDto<Project>;

public record ProjectResponseDto
    (
        Guid Id,
        Guid CongregationId,
        string CategoryName,
        string ManagerName,
        string Name,
        decimal TargetAmount,
        ProjectStatus Status,
        DateOnly StartDate,
        DateOnly? EndDate,
        string? Description
    ) : IResponseDto<Project>;

public record ProjectListResponseDto
    (
        Guid Id,
        Guid CongregationId,
        string CategoryName,
        string ManagerName,
        string Name,
        decimal TargetAmount,
        ProjectStatus Status
    ) : IListResponseDto<Project>;

public record ProjectUpdateDto
    (
        Guid Id,
        Guid CongregationId,
        string Name,
        decimal TargetAmount,
        ProjectStatus Status,
        DateOnly StartDate,
        DateOnly? EndDate,
        string? Description
    ) : IUpdateDto<Project>;

public record ProjectDeleteDto
    (
        Guid Id,
        Guid CongregationId
    ) : ISoftDeleteDto<Project>;
