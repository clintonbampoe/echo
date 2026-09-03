using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TransactionCategoryCreateDto : IReferenceCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryUpdateDto : IReferenceUpdateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TransactionType CategoryType { get; init; }
}
