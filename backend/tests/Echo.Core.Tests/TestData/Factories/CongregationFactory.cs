using Echo.Core.Dtos;
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

    public static List<Congregation> NewEntityList()
    {
        var res = new List<Congregation>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
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
}
