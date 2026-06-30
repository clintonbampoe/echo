using Echo.Core.Dtos.Interfaces;
using Echo.Domain.Enums;

namespace Echo.Core.Dtos;

public record TransactionCategoryCreateDto : IReferenceCreateDto
{
    public required string Name { get; init; }
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryUpdateDto : IReferenceUpdateDto
{
    public required string Name { get; init; }
    public TransactionType CategoryType { get; init; }
}

public record TransactionCategoryResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public TransactionType CategoryType { get; init; }
}
