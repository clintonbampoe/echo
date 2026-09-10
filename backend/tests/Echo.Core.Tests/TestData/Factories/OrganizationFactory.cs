using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class OrganizationFactory
{
    public static Organization NewEntity()
    {
        return new Organization()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Name = "Org-01",
            Description = "Description-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<Organization> NewEntityList()
    {
        var res = new List<Organization>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static OrganizationCreateDto NewCreateDto()
    {
        return new OrganizationCreateDto()
        {
            Name = "OrgCreateDto-01",
            Description = "DescCreateDto-01",
        };
    }

    public static OrganizationUpdateDto NewUpdateDto()
    {
        return new OrganizationUpdateDto()
        {
            Name = Constants.DefaultName,
            Description = "OrgUpdateDto-01",
        };
    }

    public static OrganizationUpdateDto NewUpdateDtoWithNullValues()
    {
        return new OrganizationUpdateDto() { Name = null, Description = null };
    }
}
