using Api.Core.Dtos.Interfaces;

namespace Api.Core.Dtos;

public record AttendanceContextCreateDto(string Name, int AttendanceTypeId) : IReferenceCreateDto;

public record AttendanceContextUpdateDto(string Name) : IReferenceUpdateDto;

public record AttendanceContextResponseDto(int Id, string Name, string AttendanceTypeName)
    : IReferenceResponseDto;
