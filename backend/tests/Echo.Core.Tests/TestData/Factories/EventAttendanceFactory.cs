using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class EventAttendanceFactory
{
    public static EventAttendance NewEntity()
    {
        return new EventAttendance()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            MemberId = Constants.DefaultGuid,
            Member = MemberFactory.NewEntity(),
            EventId = Constants.DefaultGuid,
            Event = EventFactory.NewEntity(),
            CheckInTime = Constants.DefaultTimeOnly,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static EventAttendanceCreateDto NewCreateDto()
    {
        return new EventAttendanceCreateDto()
        {
            MemberId = Constants.DefaultGuid,
            EventId = Constants.DefaultGuid,
            CheckInTime = new TimeOnly(9, 0),
        };
    }

    public static EventAttendanceUpdateDto NewUpdateDto()
    {
        return new EventAttendanceUpdateDto()
        {
            CheckInTime = Constants.DefaultTimeOnly,
        };
    }

    public static EventAttendanceUpdateDto NewUpdateDtoWithNullValues()
    {
        return new EventAttendanceUpdateDto()
        {
            CheckInTime = null
        };
    }
}
