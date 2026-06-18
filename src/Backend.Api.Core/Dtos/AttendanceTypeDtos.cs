using Backend.Api.Core.Dtos.Interfaces;

namespace Backend.Api.Core.Dtos;

public record AttendanceTypeCreateDto(string Name) : IReferenceCreateDto;

public record AttendanceTypeUpdateDto(string Name) : IReferenceUpdateDto;

public record AttendanceTypeResponseDto(int Id, string Name)
: IReferenceResponseDto;
