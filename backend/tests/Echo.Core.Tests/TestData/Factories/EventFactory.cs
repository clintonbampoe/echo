using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class EventFactory
{
    public static Event NewEntity()
    {
        return new Event()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            OrganizationId = Constants.DefaultGuid,
            Organization = OrganizationFactory.NewEntity(),
            OrganizerId = Constants.DefaultGuid,
            Organizer = MemberFactory.NewEntity(),
            Name = "Event-01",
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            StartTime = Constants.DefaultTimeOnly,
            EndTime = Constants.DefaultTimeOnly,
            Location = "Loc-01",
            Capacity = 100,
            Description = "Desc-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static EventCreateDto NewCreateDto()
    {
        return new EventCreateDto()
        {
            OrganizationId = Constants.DefaultGuid,
            OrganizerId = Constants.DefaultGuid,
            Name = "EventCreateDto-01",
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            StartTime = Constants.DefaultTimeOnly,
            EndTime = Constants.DefaultTimeOnly,
            Location = "LocCreate-01",
            Capacity = 100,
            Description = "DescCreate-01",
        };
    }

    public static EventUpdateDto NewUpdateDto()
    {
        return new EventUpdateDto()
        {
            OrganizationId = Constants.DefaultGuid,
            OrganizerId = Constants.DefaultGuid,
            Name = "EventUpdate-01",
            StartDate = Constants.DefaultDateOnly,
            EndDate = Constants.DefaultDateOnly,
            StartTime = Constants.DefaultTimeOnly,
            EndTime = Constants.DefaultTimeOnly,
            Location = Constants.DefaultLocation,
            Capacity = 100,
            Description = "DescUpdate-01",
        };
    }

    public static EventUpdateDto NewUpdateDtoWithNullValues()
    {
        return new EventUpdateDto()
        {
            OrganizationId = null,
            OrganizerId = null,
            Name = null,
            StartDate = null,
            EndDate = null,
            StartTime = null,
            EndTime = null,
            Location = null,
            Capacity = null,
            Description = null
        };
    }
}
