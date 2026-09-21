using Echo.Core.Events;
using Echo.Core.Tests.Members;
using Echo.Domain.Events;

namespace Echo.Core.Tests.Events;

public static class EventRegistrationFactory
{
    public static EventRegistration NewEntity()
    {
        return new EventRegistration()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            MemberId = Constants.DefaultGuid,
            Member = MemberFactory.NewEntity(),
            EventId = Constants.DefaultGuid,
            Event = EventFactory.NewEntity(),
            RegistrationDate = Constants.DefaultDateOnly,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static EventRegistrationCreateDto NewCreateDto()
    {
        return new EventRegistrationCreateDto()
        {
            MemberId = Constants.DefaultGuid,
            EventId = Constants.DefaultGuid,
            RegistrationDate = Constants.DefaultDateOnly,
        };
    }

    public static EventRegistrationUpdateDto NewUpdateDto()
    {
        return new EventRegistrationUpdateDto()
        {
            RegistrationDate = Constants.DefaultDateOnly,
        };
    }

    public static EventRegistrationUpdateDto NewUpdateDtoWithNullValues()
    {
        return new EventRegistrationUpdateDto()
        {
            RegistrationDate = Constants.DefaultDateOnly
        };
    }
}
