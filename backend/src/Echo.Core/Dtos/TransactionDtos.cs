using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TransactionCreateDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public TransactionType TransactionType { get; init; }
    public DateOnly TransactionDate { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TransactionUpdateDto
{
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    public TransactionType? TransactionType { get; init; }
    public DateOnly? TransactionDate { get; init; }

    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record TransactionResponseDto
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

public record TransactionCursor
{
    public DateOnly TransactionDate { get; init; }
    public Guid Id { get; init; }
}

public record TransactionFilters
{
    public DateOnly? Date { get; init; }
    public TransactionType? TransactionType { get; init; }
    public int? CategoryId { get; init; }
}
