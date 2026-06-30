using Echo.Core.Dtos.Interfaces;

namespace Echo.Core.Dtos;

public record AttendanceContextCreateDto : IReferenceCreateDto
{
    public required string Name { get; init; }
    public int AttendanceTypeId { get; init; }
}

public record AttendanceContextUpdateDto : IReferenceUpdateDto
{
    public required string Name { get; init; }
}

public record AttendanceContextResponseDto : IReferenceResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string AttendanceTypeName { get; init; }
}
