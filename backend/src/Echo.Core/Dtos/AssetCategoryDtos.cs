using System.ComponentModel.DataAnnotations;

namespace Echo.Core.Dtos;

public record AssetCategoryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record AssetCategoryUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }
}

public record AssetCategoryResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}

public record AssetCategorySearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
