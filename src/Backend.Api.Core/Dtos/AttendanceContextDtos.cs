using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record AttendanceContextCreateDto(string Name, int AttendanceTypeId)
    : IReferenceCreateDto<AttendanceContext>;

public record AttendanceContextUpdateDto(string Name) : IReferenceUpdateDto<AttendanceContext>;

public record AttendanceContextResponseDto(int Id, string Name, string AttendanceTypeName)
    : IReferenceResponseDto<AttendanceContext>;
