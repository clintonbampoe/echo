using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceTypeMapping;

public interface IAttendanceTypeMapper
{
    AttendanceTypeResponseDto ToDto(AttendanceType entity);
    AttendanceType ToEntity(AttendanceTypeCreateDto dto);
    void Patch(AttendanceTypeUpdateDto dto, AttendanceType entity);
}
