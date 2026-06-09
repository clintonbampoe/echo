using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TitheCreateDto : ICreateDto<Tithe>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public int Decimal { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
}

public record TitheResponseDto : IResponseDto<Tithe>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public string Member { get; init; } = string.Empty;
    public int Decimal { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
}

public record TitheListResponseDto : IListResponseDto<Tithe>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Member { get; init; } = string.Empty;
    public int Decimal { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
}

public record TitheUpdateDto : IUpdateDto<Tithe>
{
    public Guid CongregationId { get; init; }
    public Guid MemberId { get; init; }
    public int Decimal { get; init; }
    public int ForYear { get; init; }
    public MonthOfYear ForMonth { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateOnly CollectionDate { get; init; }
    public string? Description { get; init; }
}

public record TitheDeleteDto : ISoftDeleteDto<Tithe>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}

public record TitheMonthlySummaryDto : ISummaryDto<Tithe>
{
    public int Year { get; init; }
    public IEnumerable<TitheMonthlyAggregateDto> Months { get; init; } = [];
}

public record TitheMonthlyAggregateDto
{
    public MonthOfYear Month { get; init; }
    public decimal Total { get; init; }
}
