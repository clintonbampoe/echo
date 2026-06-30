using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record AttendanceTypeCreateDto : IReferenceCreateDto
{
    public required string Name { get; init; }
}

public record AttendanceTypeUpdateDto : IReferenceUpdateDto
{
    public required string Name { get; init; }
}

public record AttendanceTypeResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}
