using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record AssetCategoryCreateDto : IReferenceCreateDto
{
    public required string Name { get; init; }
}

public record AssetCategoryUpdateDto : IReferenceUpdateDto
{
    public required string Name { get; init; }
}

public record AssetCategoryResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
