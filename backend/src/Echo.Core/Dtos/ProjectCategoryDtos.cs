using System.ComponentModel.DataAnnotations;

namespace Echo.Core.Dtos;

public record ProjectCategoryCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record ProjectCategoryUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }
}

public record ProjectCategoryResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}

public record ProjectCategorySearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
