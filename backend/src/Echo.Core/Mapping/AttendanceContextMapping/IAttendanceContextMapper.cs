using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Mapping.AttendanceContextMapping;

public interface IAttendanceContextMapper
{
    AttendanceContextResponseDto ToDto(AttendanceContext entity);
    AttendanceContext ToEntity(AttendanceContextCreateDto dto);
    List<AttendanceContextResponseDto> ToListDto(List<AttendanceContext> entities);

    List<AttendanceContextSearchResultDto> ToSearchDto(List<AttendanceContext> entities);
    void Patch(AttendanceContextUpdateDto dto, AttendanceContext entity);
}
