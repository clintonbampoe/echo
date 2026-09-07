using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceMapping;

public interface IAttendanceMapper
{
    AttendanceResponseDto ToDto(Attendance entity);
    Attendance ToEntity(AttendanceCreateDto dto);
    void Patch(AttendanceUpdateDto dto, Attendance entity);
}
