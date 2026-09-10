using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class AttendanceFactory
{
    public static Attendance NewEntity()
    {
        return new Attendance()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            AttendanceContextId = Constants.DefaultInt,
            AttendanceContext = AttendanceContextFactory.NewEntity(),
            MemberId = Constants.DefaultGuid,
            Member = MemberFactory.NewEntity(),
            ForDate = Constants.DefaultDateOnly,
            AttendeeType = default,
            CheckInTime = Constants.DefaultTimeOnly,
            Description = "Description-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static AttendanceCreateDto NewCreateDto()
    {
        return new AttendanceCreateDto()
        {
            AttendanceContextId = Constants.DefaultInt,
            MemberId = Constants.DefaultGuid,
            AttendeeType = default,
            ForDate = Constants.DefaultDateOnly,
            CheckInTime = Constants.DefaultTimeOnly,
            Description = "AttendanceCreateDescription-01",
        };
    }

    public static AttendanceUpdateDto NewUpdateDto()
    {
        return new AttendanceUpdateDto()
        {
            AttendanceContextId = Constants.DefaultInt,
            MemberId = Constants.DefaultGuid,
            AttendeeType = AttendeeType.Member,
            ForDate = Constants.DefaultDateOnly,
            CheckInTime = Constants.DefaultTimeOnly,
            Description = "AttendanceUpdateDescription-01",
        };
    }

    public static AttendanceUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AttendanceUpdateDto()
        {
            AttendanceContextId = null,
            MemberId = null,
            AttendeeType = null,
            ForDate = null,
            CheckInTime = null,
            Description = null,
        };
    }
}
