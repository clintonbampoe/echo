using System.ComponentModel.DataAnnotations;

namespace Echo.Core.Dtos;

public record AttendanceContextCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public int AttendanceTypeId { get; init; }
}

public record AttendanceContextUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }
}

public record AttendanceContextResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string AttendanceTypeName { get; init; }
}

public record AttendanceContextSearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
