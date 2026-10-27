using System.ComponentModel.DataAnnotations;

namespace Echo.Application.Assets;

public record AssetCategoryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(40)]
    public string? Icon { get; init; }
}

public record AssetCategoryUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(40)]
    public string? Icon { get; init; }
}

public record AssetCategoryResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Icon { get; init; }
    public int AssetCount { get; init; }
}

public record AssetCategorySearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
