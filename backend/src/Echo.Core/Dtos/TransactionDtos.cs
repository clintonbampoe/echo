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

public record TransactionUpdateDto : IPrimaryUpdateDto
{
    public int CategoryId { get; init; }
    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}

public record TransactionListResponseDto : IPrimaryListResponseDto, Shared.Dtos.Interfaces.IPrimaryListResponseDto
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
