using Api.Core.Dtos.Interfaces;

namespace Api.Core.Dtos;

public record AttendanceTypeCreateDto(string Name) : IReferenceCreateDto;

public record AttendanceTypeUpdateDto(string Name) : IReferenceUpdateDto;

public record AttendanceTypeResponseDto(int Id, string Name)
: IReferenceResponseDto;
