using Echo.Domain.Events;
using Riok.Mapperly.Abstractions;

namespace Echo.Application.Events;

[Mapper]
public partial class EventAttendanceMapper : IEventAttendanceMapper
{
    public EventAttendanceResponseDto ToDto(EventAttendance entity) =>
        new EventAttendanceResponseDto
        {
            Id = entity.Id,
            MemberId = entity.MemberId,
            MemberName = entity.Member.Person.Name,
            EventId = entity.EventId,
            EventName = entity.Event.Name,
            CheckInTime = entity.CheckInTime,
            CreatedAt = entity.CreatedAt,
        };

    [MapperIgnoreTarget(nameof(EventAttendance.Congregation))]
    [MapperIgnoreTarget(nameof(EventAttendance.CongregationId))]
    [MapperIgnoreTarget(nameof(EventAttendance.Id))]
    [MapperIgnoreTarget(nameof(EventAttendance.Member))]
    [MapperIgnoreTarget(nameof(EventAttendance.Event))]
    [MapperIgnoreTarget(nameof(EventAttendance.CreatedAt))]
    [MapperIgnoreTarget(nameof(EventAttendance.DeletedAt))]
    public partial EventAttendance ToEntity(EventAttendanceCreateDto dto);

    public partial List<EventAttendanceResponseDto> ToListDto(List<EventAttendance> entities);

    public void Patch(EventAttendanceUpdateDto dto, EventAttendance entity)
    {
        if (dto.CheckInTime != null)
            entity.CheckInTime = dto.CheckInTime.Value;
    }
}
