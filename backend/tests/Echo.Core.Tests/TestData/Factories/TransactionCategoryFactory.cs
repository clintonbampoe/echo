using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class TransactionCategoryFactory
{
    public static TransactionCategory NewEntity()
    {
        return new TransactionCategory()
        {
            Id = Constants.DefaultInt,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Name = "Cat-01",
            CategoryType = TransactionType.Income,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static List<TransactionCategory> NewEntityList()
    {
        var res = new List<TransactionCategory>();
        for (int i = 0; i < 5; i++)
        {
            res.Add(NewEntity());
        }

        return res;
    }

    public static TransactionCategoryCreateDto NewCreateDto()
    {
        return new TransactionCategoryCreateDto()
        {
            Name = "CatCreateDto-01",
            CategoryType = TransactionType.Income,
        };
    }

    public static TransactionCategoryUpdateDto NewUpdateDto()
    {
        return new TransactionCategoryUpdateDto()
        {
            Name = "CatCreateDto-01",
            CategoryType = TransactionType.Income,
        };
    }

    public static TransactionCategoryUpdateDto NewUpdateDtoWithNullValues()
    {
        return new TransactionCategoryUpdateDto() { Name = null, CategoryType = null };
    }
}
