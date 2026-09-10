using System.ComponentModel.DataAnnotations;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TransactionCategoryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    public TransactionType? CategoryType { get; init; }
}

public record TransactionCategoryResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategorySearchResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required TransactionType Type { get; init; }
}
