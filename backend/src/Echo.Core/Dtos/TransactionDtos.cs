using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TransactionCreateDto : IPrimaryCreateDto
{
    public int CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record FinanceSummaryDto
{
    public decimal TotalIncome { get; init; }
    public decimal IncomeDelta { get; init; }
    public decimal TotalExpenditure { get; init; }
    public decimal ExpenditureDelta { get; init; }
    public decimal NetBalance { get; init; }
    public decimal NetBalanceDelta { get; init; }
}

public record TransactionStreamDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public decimal PercentOfTotal { get; init; }
}

public record TransactionUpdateDto : IPrimaryUpdateDto
{
    public int CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record TransactionListResponseDto : IPrimaryListResponseDto, Application.Dtos.Interfaces.IPrimaryListResponseDto
{
    public Guid Id { get; init; }
    public required string CategoryName { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
}

public record TransactionResponseDto : IPrimaryResponseDto
{
    public Guid Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record FinanceStreamsDto
{
    public required List<TransactionStreamDto> IncomeStreams { get; init; }
    public required List<TransactionStreamDto> ExpenditureStreams { get; init; }
}
