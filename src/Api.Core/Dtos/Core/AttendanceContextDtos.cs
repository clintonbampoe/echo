using Api.Core.Dtos.Core.Interfaces;

namespace Api.Core.Dtos.Core;

public record AttendanceContextCreateDto(string Name, int AttendanceTypeId) : IReferenceCreateDto;

public record AttendanceContextUpdateDto(string Name) : IReferenceUpdateDto;

public record AttendanceContextResponseDto(int Id, string Name, string AttendanceTypeName)
    : IReferenceResponseDto;
