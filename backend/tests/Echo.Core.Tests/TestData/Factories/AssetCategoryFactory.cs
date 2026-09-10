using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Tests.TestData.Factories;

public static class AssetCategoryFactory
{
    public static AssetCategory NewEntity()
    {
        return new AssetCategory()
        {
            Id = Constants.DefaultInt,
            Name = "AssetCategory-01",
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<AssetCategory> NewEntityList()
    {
        var res = new List<AssetCategory>();

        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static AssetCategoryCreateDto NewCreateDto()
    {
        return new AssetCategoryCreateDto { Name = "AssetCategoryCreate-01" };
    }

    public static AssetCategoryUpdateDto NewUpdateDto()
    {
        return new AssetCategoryUpdateDto { Name = "AssetCategoryUpdate-01" };
    }

    public static AssetCategoryUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AssetCategoryUpdateDto { Name = null };
    }
}
