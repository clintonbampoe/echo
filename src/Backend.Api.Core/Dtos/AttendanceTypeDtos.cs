using Backend.Api.Core.Dtos.Interfaces;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Dtos;

public record AttendanceTypeCreateDto(string Name) : IReferenceCreateDto<AttendanceType>;

public record AttendanceTypeUpdateDto(string Name) : IReferenceUpdateDto<AttendanceType>;

public record AttendanceTypeResponseDto(int Id, string Name)
    : IReferenceResponseDto<AttendanceType>;
