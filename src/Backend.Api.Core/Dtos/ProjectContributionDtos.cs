using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record ProjectContributionCreateDto : ICreateDto<ProjectContribution>
{
    public Guid CongregationId { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Description { get; init; }
}

public record ProjectContributionResponseDto : IResponseDto<ProjectContribution>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid ProjectId { get; init; }
    public string Project { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Description { get; init; }
}

public record ProjectContributionListResponseDto : IListResponseDto<ProjectContribution>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Project { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
}

public record ProjectContributionUpdateDto : IUpdateDto<ProjectContribution>
{
    public Guid CongregationId { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Description { get; init; }
}

public record ProjectContributionDeleteDto : ISoftDeleteDto<ProjectContribution>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
