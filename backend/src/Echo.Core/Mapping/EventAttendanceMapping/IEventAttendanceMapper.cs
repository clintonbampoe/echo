using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.EventAttendanceMapping;

public interface IEventAttendanceMapper
{
    EventAttendanceResponseDto ToDto(EventAttendance entity);
    EventAttendance ToEntity(EventAttendanceCreateDto dto);
    void Patch(EventAttendanceUpdateDto dto, EventAttendance entity);
}
