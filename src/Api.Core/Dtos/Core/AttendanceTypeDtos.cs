using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record AttendanceTypeCreateDto(string Name) : IReferenceCreateDto;

public record AttendanceTypeUpdateDto(string Name) : IReferenceUpdateDto;

public record AttendanceTypeResponseDto(int Id, string Name)
: IReferenceResponseDto;
