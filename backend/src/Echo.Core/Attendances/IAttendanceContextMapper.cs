using Echo.Domain.Attendances;

namespace Echo.Core.Attendances;

public interface IAttendanceContextMapper
{
    AttendanceContextResponseDto ToDto(AttendanceContext entity);
    AttendanceContext ToEntity(AttendanceContextCreateDto dto);
    List<AttendanceContextResponseDto> ToListDto(
        List<AttendanceContext> entities
    );

    List<AttendanceContextSearchResultDto> ToSearchDto(
        List<AttendanceContext> entities
    );
    void Patch(AttendanceContextUpdateDto dto, AttendanceContext entity);
}
