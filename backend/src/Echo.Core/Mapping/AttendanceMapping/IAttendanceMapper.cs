using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceMapping;

public interface IAttendanceMapper
{
    AttendanceResponseDto ToDto(Attendance entity);
    Attendance ToEntity(AttendanceCreateDto dto);
    List<AttendanceResponseDto> ToListDto(List<Attendance> entities);

    void Patch(AttendanceUpdateDto dto, Attendance entity);
}
