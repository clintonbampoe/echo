using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Echo.Core.Mapping.EventAttendanceMapping;

[Mapper]
public partial class EventAttendanceMapper : IEventAttendanceMapper
{
    [MapperIgnoreSource(nameof(entity.Congregation))]
    [MapperIgnoreSource(nameof(entity.CongregationId))]
    [MapperIgnoreSource(nameof(entity.DeletedAt))]
    [MapProperty(
        nameof(EventAttendance.Member.Name),
        nameof(EventAttendanceResponseDto.MemberName)
    )]
    [MapProperty(nameof(EventAttendance.Event.Name), nameof(EventAttendanceResponseDto.EventName))]
    public partial EventAttendanceResponseDto ToDto(EventAttendance entity);

    [MapperIgnoreTarget(nameof(EventAttendance.Congregation))]
    [MapperIgnoreTarget(nameof(EventAttendance.CongregationId))]
    [MapperIgnoreTarget(nameof(EventAttendance.Id))]
    [MapperIgnoreTarget(nameof(EventAttendance.Member))]
    [MapperIgnoreTarget(nameof(EventAttendance.Event))]
    [MapperIgnoreTarget(nameof(EventAttendance.CreatedAt))]
    [MapperIgnoreTarget(nameof(EventAttendance.DeletedAt))]
    public partial EventAttendance ToEntity(EventAttendanceCreateDto dto);

    public void Patch(EventAttendanceUpdateDto dto, EventAttendance entity)
    {
        if (dto.CheckInTime != null)
            entity.CheckInTime = dto.CheckInTime.Value;
    }
}
