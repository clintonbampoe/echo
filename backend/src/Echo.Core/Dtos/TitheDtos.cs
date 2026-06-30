using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TitheCreateDto : IPrimaryCreateDto
{
    public Guid MemberId { get; init; }
    public decimal Amount { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
}

public record TitheUpdateDto : IPrimaryUpdateDto
{
    public Guid MemberId { get; init; }
    public decimal Amount { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
}

public record TitheListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string MemberName { get; init; }
    public decimal Amount { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
}

public record TitheResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public required string MemberName { get; init; }
    public decimal Amount { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}
