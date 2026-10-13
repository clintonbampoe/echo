using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public class AttendanceMapper : IAttendanceMapper
{
    public AttendanceResponseDto ToDto(Attendance entity) =>
        new()
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            SessionDate = entity.Session.Date,
            SessionName = entity.Session.Name,
            SessionLabelId = entity.Session.SessionLabelId,
            SessionLabelName = entity.Session.SessionLabel.Name,
            PersonId = entity.PersonId,
            PersonName = entity.Person.Name,
            PersonKind = entity.PersonKindAtTime,
            CheckedInAt = entity.CheckedInAt,
            RecordedBy = entity.RecordedBy,
            CreatedAt = entity.CreatedAt,
        };

    public Attendance ToEntity(AttendanceCreateDto dto) =>
        new()
        {
            SessionId = dto.SessionId,
            PersonId = dto.PersonId,
            CheckedInAt = dto.CheckedInAt,
        };

    public List<AttendanceResponseDto> ToListDto(List<Attendance> entities) =>
        [.. entities.Select(ToDto)];

    public void Patch(AttendanceUpdateDto dto, Attendance entity)
    {
        if (dto.CheckedInAt.HasValue)
            entity.CheckedInAt = dto.CheckedInAt.Value;
    }
}
