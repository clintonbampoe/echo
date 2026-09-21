using Echo.Domain.Events;

namespace Echo.Core.Events;

public interface IEventAttendanceMapper
{
    EventAttendanceResponseDto ToDto(EventAttendance entity);
    EventAttendance ToEntity(EventAttendanceCreateDto dto);
    List<EventAttendanceResponseDto> ToListDto(List<EventAttendance> entities);
    void Patch(EventAttendanceUpdateDto dto, EventAttendance entity);
}
