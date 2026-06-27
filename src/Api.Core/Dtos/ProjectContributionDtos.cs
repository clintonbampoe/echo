using Api.Core.Dtos.Interfaces;
using Api.Core.Enums;

namespace Api.Core.Dtos;

public record ProjectContributionCreateDto(
    Guid ProjectId,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description
) : IPrimaryCreateDto;

public record ProjectContributionUpdateDto(
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description
) : IPrimaryUpdateDto;

public record ProjectContributionListResponseDto(
    Guid Id,
    string ProjectName,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod
) : IPrimaryListResponseDto;

public record ProjectContributionResponseDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    decimal Amount,
    DateOnly DateContributed,
    PaymentMethod PaymentMethod,
    string? Description,
    DateTime CreatedAt
) : IPrimaryResponseDto;
