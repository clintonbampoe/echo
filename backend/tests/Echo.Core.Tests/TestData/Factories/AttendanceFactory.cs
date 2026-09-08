using Echo.Core.Dtos;
using Echo.Core.Tests.Helpers;
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
            MemberId = null,
            Member = null,
            GuestName = "Guest-01",
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
            MemberId = null,
            GuestName = "AttendanceCreate-01",
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
            GuestName = "AttendanceUpdate-01",
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
            GuestName = null,
            AttendeeType = null,
            ForDate = null,
            CheckInTime = null,
            Description = null,
        };
    }

    public static AttendanceUpdateDto NewUpdateDtoWithRandomValues()
    {
        return new AttendanceUpdateDto()
        {
            AttendanceContextId = 99,
            MemberId = Guid.NewGuid(),
            GuestName = RandomGenerators.String(10),
            AttendeeType = AttendeeType.Guest,
            ForDate = new DateOnly(2024, 6, 15),
            CheckInTime = new TimeOnly(10, 30),
            Description = RandomGenerators.String(20),
        };
    }
}
