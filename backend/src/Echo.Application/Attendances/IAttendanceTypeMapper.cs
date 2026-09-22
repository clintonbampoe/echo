using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public interface IAttendanceTypeMapper
{
    AttendanceTypeResponseDto ToDto(AttendanceType entity);
    AttendanceType ToEntity(AttendanceTypeCreateDto dto);
    List<AttendanceTypeResponseDto> ToListDto(List<AttendanceType> entities);

    List<AttendanceTypeSearchResultDto> ToSearchDto(List<AttendanceType> entities);
    void Patch(AttendanceTypeUpdateDto dto, AttendanceType entity);
}
