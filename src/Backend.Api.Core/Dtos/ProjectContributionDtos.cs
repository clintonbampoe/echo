using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record ProjectContributionCreateDto(
    Guid ProjectId,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description
) : IPrimaryCreateDto<ProjectContribution>;

public record ProjectContributionUpdateDto(
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description
) : IPrimaryUpdateDto<ProjectContribution>;

public record ProjectContributionListResponseDto(
    Guid Id,
    string ProjectName,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod
) : IPrimaryListResponseDto<ProjectContribution>;

public record ProjectContributionResponseDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto<ProjectContribution>;
