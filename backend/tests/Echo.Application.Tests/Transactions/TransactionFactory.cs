using Echo.Application.Transactions;
using Echo.Domain.Transactions;

namespace Echo.Application.Tests.Transactions;

public static class TransactionFactory
{
    public static Transaction NewEntity()
    {
        return new Transaction()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            CategoryId = Constants.DefaultInt,
            Category = TransactionCategoryFactory.NewEntity(),
            TransactionType = TransactionType.Income,
            TransactionDate = Constants.DefaultDateOnly,
            Amount = 50.0m,
            Description = "Trans-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static TransactionCreateDto NewCreateDto()
    {
        return new TransactionCreateDto()
        {
            CategoryId = Constants.DefaultInt,
            TransactionType = TransactionType.Income,
            TransactionDate = Constants.DefaultDateOnly,
            Amount = 50.0m,
            Description = "Trans-01",
        };
    }

    public static TransactionUpdateDto NewUpdateDto()
    {
        return new TransactionUpdateDto()
        {
            CategoryId = Constants.DefaultInt,
            TransactionType = TransactionType.Income,
            TransactionDate = Constants.DefaultDateOnly,
            Amount = 50.0m,
            Description = "Trans-01",
        };
    }

    public static TransactionUpdateDto NewUpdateDtoWithNullValues()
    {
        return new TransactionUpdateDto()
        {
            CategoryId = null,
            TransactionType = null,
            TransactionDate = null,
            Amount = null,
            Description = null,
        };
    }
}
