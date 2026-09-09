using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceContextMapping;

public interface IAttendanceContextMapper
{
    AttendanceContextResponseDto ToDto(AttendanceContext entity);
    AttendanceContext ToEntity(AttendanceContextCreateDto dto);
    void Patch(AttendanceContextUpdateDto dto, AttendanceContext entity);
}
