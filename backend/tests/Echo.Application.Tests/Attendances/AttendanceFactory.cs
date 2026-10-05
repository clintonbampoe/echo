using Echo.Application.Attendances;
using Echo.Application.Tests.Congregations;
using Echo.Application.Tests.Members;
using Echo.Domain.Attendances;

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
            AttendanceTypeId = Constants.DefaultInt,
            AttendanceType = AttendanceTypeFactory.NewEntity(),
            PersonId = Constants.DefaultGuid,
            Person = PersonFactory.NewMemberPerson(),
            Date = Constants.DefaultDateOnly,
            CheckInTime = Constants.DefaultTimeOnly,
            Notes = "Notes-01",
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
            AttendanceTypeId = Constants.DefaultInt,
            PersonId = Constants.DefaultGuid,
            Date = Constants.DefaultDateOnly,
            CheckInTime = Constants.DefaultTimeOnly,
            Notes = "AttendanceCreateNotes-01",
        };
    }

    public static AttendanceUpdateDto NewUpdateDto()
    {
        return new AttendanceUpdateDto
        {
            CheckInTime = Constants.DefaultTimeOnly,
            Notes = "AttendanceUpdateNotes-01",
        };
    }

    public static AttendanceUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AttendanceUpdateDto { CheckInTime = null, Notes = null };
    }
}
