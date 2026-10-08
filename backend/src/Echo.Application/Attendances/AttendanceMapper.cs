using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public class AttendanceMapper : IAttendanceMapper
{
    public AttendanceResponseDto ToDto(Attendance entity)
    {
        return new AttendanceResponseDto
        {
            Id = entity.Id,
            AttendanceTypeId = entity.AttendanceTypeId,
            AttendanceTypeName = entity.AttendanceType.Name,
            PersonId = entity.PersonId,
            PersonName = entity.Person.Name,
            PersonKind = entity.Person.Kind,
            Date = entity.Date,
            CheckInTime = entity.CheckInTime,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
        };
    }

    public Attendance ToEntity(AttendanceCreateDto dto)
    {
        return new Attendance
        {
            AttendanceTypeId = dto.AttendanceTypeId,
            PersonId = dto.PersonId,
            Date = dto.Date,
            CheckInTime = dto.CheckInTime,
            Notes = dto.Notes,
        };
    }

    public List<AttendanceResponseDto> ToListDto(List<Attendance> entities) =>
        entities.Select(ToDto).ToList();

    public void Patch(AttendanceUpdateDto dto, Attendance entity)
    {
        if (dto.CheckInTime.HasValue)
            entity.CheckInTime = dto.CheckInTime.Value;
        if (dto.Notes != null)
            entity.Notes = dto.Notes;
    }
}
