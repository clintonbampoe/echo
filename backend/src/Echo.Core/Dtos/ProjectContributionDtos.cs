using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record ProjectContributionCreateDto : IPrimaryCreateDto
{
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public required string ? Description { get; init; }
}

public record ProjectContributionUpdateDto : IPrimaryUpdateDto
{
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public required string ? Description { get; init; }
}

public record ProjectContributionListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string  ProjectName { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
}

public record ProjectContributionResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public required string  ProjectName { get; init; }
    public decimal Amount { get; init; }
    public DateOnly DateContributed { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public required string ? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
