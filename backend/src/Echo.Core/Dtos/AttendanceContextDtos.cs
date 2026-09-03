using System.ComponentModel.DataAnnotations;
using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record AttendanceContextCreateDto : IReferenceCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public int AttendanceTypeId { get; init; }
}

public record AttendanceContextUpdateDto : IReferenceUpdateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}

public record AttendanceContextResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string AttendanceTypeName { get; init; }
}
