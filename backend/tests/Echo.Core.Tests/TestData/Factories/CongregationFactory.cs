using Echo.Core.Dtos;
using Echo.Core.Tests.Helpers;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class CongregationFactory
{
    public static Congregation NewEntity()
    {
        return new Congregation()
        {
            Id = Constants.DefaultGuid,
            Name = "Congregation-01",
            PhoneNumber = Constants.DefaultPhoneNumber,
            EmailAddress = Constants.DefaultEmailAddress,
            PostalAddress = Constants.DefaultPostalAddress,
            WebsiteUrl = Constants.DefaultWebsiteUrl,
            Region = default,
            OrgType = default,
            City = Constants.DefaultCity,
            Town = Constants.DefaultTown,
            GpsAddress = Constants.DefaultGpsAddress,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static CongregationCreateDto NewCreateDto()
    {
        return new CongregationCreateDto()
        {
            Name = "CongregationCreate-01",
            PhoneNumber = Constants.DefaultPhoneNumber,
            EmailAddress = Constants.DefaultEmailAddress,
            PostalAddress = Constants.DefaultPostalAddress,
            WebsiteUrl = Constants.DefaultWebsiteUrl,
            Region = Region.WesternNorth,
            OrgType = ReligiousOrganizationType.Other,
            City = Constants.DefaultCity,
            Town = Constants.DefaultTown,
            GpsAddress = Constants.DefaultGpsAddress,
        };
    }

    public static CongregationUpdateDto NewUpdateDto()
    {
        return new CongregationUpdateDto()
        {
            Name = "CongregationUpdate-01",
            PhoneNumber = Constants.DefaultPhoneNumber,
            EmailAddress = Constants.DefaultEmailAddress,
            PostalAddress = Constants.DefaultPostalAddress,
            WebsiteUrl = Constants.DefaultWebsiteUrl,
            Region = Region.WesternNorth,
            OrgType = ReligiousOrganizationType.Other,
            City = Constants.DefaultCity,
            Town = Constants.DefaultTown,
            GpsAddress = Constants.DefaultGpsAddress,
        };
    }

    public static CongregationUpdateDto NewUpdateDtoWithNullFields()
    {
        return new CongregationUpdateDto()
        {
            Name = null,
            PhoneNumber = null,
            EmailAddress = null,
            PostalAddress = null,
            WebsiteUrl = null,
            Region = null,
            OrgType = null,
            City = null,
            Town = null,
            GpsAddress = null,
        };
    }

    public static CongregationUpdateDto NewUpdateDtoWithRandomValues()
    {
        return new CongregationUpdateDto()
        {
            Name = "CongregationUpdate-01 With Random Values",
            PhoneNumber = RandomGenerators.String(10),
            EmailAddress = RandomGenerators.String(10),
            PostalAddress = RandomGenerators.String(10),
            WebsiteUrl = RandomGenerators.String(10),
            Region = Region.WesternNorth,
            OrgType = ReligiousOrganizationType.Other,
            City = RandomGenerators.String(10),
            Town = RandomGenerators.String(10),
            GpsAddress = RandomGenerators.String(10),
        };
    }
}
