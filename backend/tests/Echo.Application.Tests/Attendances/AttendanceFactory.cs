using Echo.Application.Attendances;
using Echo.Application.Tests.Congregations;
using Echo.Application.Tests.Members;
using Echo.Domain.Attendances;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Attendances;

public static class AttendanceFactory
{
    public static Attendance NewEntity()
    {
        return new Attendance
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            SessionId = Constants.DefaultGuid,
            Session = SessionFactory.NewEntity(),
            PersonId = Constants.DefaultGuid,
            Person = PersonFactory.NewMemberPerson(),
            PersonKindAtTime = PersonKind.Member,
            CheckedInAt = Constants.DefaultDateTime,
            RecordedBy = null,
            Recorder = null,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<Attendance> NewEntityList()
    {
        var res = new List<Attendance>();
        for (int i = 0; i < 5; i++)
            res.Add(NewEntity());
        return res;
    }

    public static AttendanceCreateDto NewCreateDto()
    {
        return new AttendanceCreateDto
        {
            SessionId = Constants.DefaultGuid,
            PersonId = Constants.DefaultGuid,
            CheckedInAt = Constants.DefaultDateTime,
        };
    }

    public static AttendanceUpdateDto NewUpdateDto()
    {
        return new AttendanceUpdateDto { CheckedInAt = Constants.DefaultDateTime };
    }

    public static AttendanceUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AttendanceUpdateDto { CheckedInAt = null };
    }
}
