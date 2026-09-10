using System.ComponentModel.DataAnnotations;

namespace Echo.Core.Dtos;

public record AttendanceTypeCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record AttendanceTypeUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }
}

public record AttendanceTypeResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}

public record AttendanceTypeSearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
