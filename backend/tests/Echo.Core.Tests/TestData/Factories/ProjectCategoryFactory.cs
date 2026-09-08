using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class ProjectCategoryFactory
{
    public static ProjectCategory NewEntity()
    {
        return new ProjectCategory()
        {
            Id = Constants.DefaultInt,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Name = "ProjectCat-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static ProjectCategoryCreateDto NewCreateDto()
    {
        return new ProjectCategoryCreateDto()
        {
            Name = "CatCreateDto-01",
        };
    }

    public static ProjectCategoryUpdateDto NewUpdateDto()
    {
        return new ProjectCategoryUpdateDto()
        {
            Name = "ProjectCat UpdateDto-01"
        };
    }

    public static ProjectCategoryUpdateDto NewUpdateDtoWithNullValues()
    {
        return new ProjectCategoryUpdateDto()
        {
            Name = null,
        };
    }
}
