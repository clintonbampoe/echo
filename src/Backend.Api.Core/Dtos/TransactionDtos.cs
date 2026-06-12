using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Enums;

namespace Backend.Api.Core.Dtos;

public record TransactionCreateDto : ICreateDto<Transaction>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record TransactionResponseDto : IResponseDto<Transaction>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public string Category { get; init; } = string.Empty;
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record TransactionListResponseDto : IListResponseDto<Transaction>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
    public string Category { get; init; } = string.Empty;
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
}

public record TransactionUpdateDto : IUpdateDto<Transaction>
{
    public Guid CongregationId { get; init; }
    public Guid CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record TransactionDeleteDto : ISoftDeleteDto<Transaction>
{
    public Guid Id { get; init; }
    public Guid CongregationId { get; init; }
}

public record TransactionSummaryDto : ISummaryDto<Transaction>
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenditure { get; init; }
    public decimal NetBalance { get; init; }
    public IEnumerable<TransactionIncomeStreamDto> IncomeStreams { get; init; } = [];
    public IEnumerable<TransactionExpenditureStreamDto> ExpenditureStreams { get; init; } = [];
}

public record TransactionStreamDto
{
    public string Category { get; init; } = string.Empty;
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
}

public record TransactionIncomeStreamDto
{
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal PercentageOfTotal { get; init; }
}

public record TransactionExpenditureStreamDto
{
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal PercentageOfTotal { get; init; }
}
