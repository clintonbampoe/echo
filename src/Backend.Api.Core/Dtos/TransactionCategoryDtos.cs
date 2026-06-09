using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TransactionCategoryCreateDto : ICreateDto<TransactionCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryResponseDto : IResponseDto<TransactionCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryListResponseDto : IListResponseDto<TransactionCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryUpdateDto : IUpdateDto<TransactionCategory>
{
    public Guid CongregationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryDeleteDto : ISoftDeleteDto<TransactionCategory>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}
