using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TitheCreateDto : IPrimaryCreateDto
{
    public Guid MemberId { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [Range(1900, 2100)]
    public int ForYear { get; init; }

    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TitheUpdateDto : IPrimaryUpdateDto
{
    public Guid MemberId { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [Range(1900, 2100)]
    public int ForYear { get; init; }

    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TitheListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
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

public record TitheMonthlyTotalDto
{
    public required MonthOfYear Month { get; init; }
    public required decimal Total { get; init; }
}
