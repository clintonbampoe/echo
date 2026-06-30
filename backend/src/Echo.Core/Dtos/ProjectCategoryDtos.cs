using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record ProjectCategoryCreateDto : IReferenceCreateDto
{
    public required string Name { get; init; }
}

public record ProjectCategoryUpdateDto : IReferenceUpdateDto
{
    public required string Name { get; init; }
}

public record ProjectCategoryResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
