using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceTypeMapping;

public interface IAttendanceTypeMapper
{
    AttendanceTypeResponseDto ToDto(AttendanceType entity);
    AttendanceType ToEntity(AttendanceTypeCreateDto dto);
    List<AttendanceTypeResponseDto> ToListDto(List<AttendanceType> entities);

    List<AttendanceTypeSearchResultDto> ToSearchDto(List<AttendanceType> entities);
    void Patch(AttendanceTypeUpdateDto dto, AttendanceType entity);
}
