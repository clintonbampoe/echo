using Echo.Domain.Attendances;

namespace Echo.Core.Attendances;

public interface IAttendanceMapper
{
    AttendanceResponseDto ToDto(Attendance entity);
    Attendance ToEntity(AttendanceCreateDto dto);
    List<AttendanceResponseDto> ToListDto(List<Attendance> entities);

    void Patch(AttendanceUpdateDto dto, Attendance entity);
}
