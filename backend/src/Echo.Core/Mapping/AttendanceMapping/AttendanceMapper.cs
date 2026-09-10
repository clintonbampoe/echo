using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.AttendanceMapping;

[Mapper]
public partial class AttendanceMapper : IAttendanceMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(
        [nameof(entity.AttendanceContext), nameof(entity.AttendanceContext.Name)],
        nameof(AttendanceResponseDto.AttendanceContextName)
    )]
    [MapProperty(
        [
            nameof(entity.AttendanceContext),
            nameof(entity.AttendanceContext.AttendanceType),
            nameof(entity.AttendanceContext.AttendanceType.Name),
        ],
        nameof(AttendanceResponseDto.AttendanceTypeName)
    )]
    public partial AttendanceResponseDto ToDto(Attendance entity);

    [MapperIgnoreTarget(nameof(Attendance.Congregation))]
    [MapperIgnoreTarget(nameof(Attendance.CongregationId))]
    [MapperIgnoreTarget(nameof(Attendance.Id))]
    [MapperIgnoreTarget(nameof(Attendance.AttendanceContext))]
    [MapperIgnoreTarget(nameof(Attendance.Member))]
    [MapperIgnoreTarget(nameof(Attendance.CreatedAt))]
    [MapperIgnoreTarget(nameof(Attendance.DeletedAt))]
    public partial Attendance ToEntity(AttendanceCreateDto dto);

    public partial List<AttendanceResponseDto> ToListDto(List<Attendance> entities);

    public void Patch(AttendanceUpdateDto dto, Attendance entity)
    {
        if (dto.AttendanceContextId.HasValue)
            entity.AttendanceContextId = dto.AttendanceContextId.Value;
        if (dto.MemberId.HasValue)
            entity.MemberId = dto.MemberId.Value;
        if (dto.AttendeeType.HasValue)
            entity.AttendeeType = dto.AttendeeType.Value;
        if (dto.ForDate.HasValue)
            entity.ForDate = dto.ForDate.Value;
        if (dto.CheckInTime.HasValue)
            entity.CheckInTime = dto.CheckInTime.Value;
        if (dto.Description != null)
            entity.Description = dto.Description;
    }
}
