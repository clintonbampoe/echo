using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record AttendanceTypeCreateDto : IReferenceCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record AttendanceTypeUpdateDto : IReferenceUpdateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record AttendanceTypeResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
