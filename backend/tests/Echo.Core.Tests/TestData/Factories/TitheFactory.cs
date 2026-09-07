using Echo.Core.Dtos;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class TitheFactory
{
    public static Tithe NewEntity()
    {
        return new Tithe()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            MemberId = Constants.DefaultGuid,
            Member = MemberFactory.NewEntity(),
            Amount = 100.0m,
            ForYear = 2026,
            ForMonth = MonthOfYear.January,
            PaymentMethod = PaymentMethod.Cash,
            CollectionDate = Constants.DefaultDateOnly,
            Description = "Tithe-01",
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static TitheCreateDto NewCreateDto()
    {
        return new TitheCreateDto()
        {
            MemberId = Constants.DefaultGuid,
            Amount = 100.0m,
            ForYear = 2026,
            ForMonth = MonthOfYear.January,
            PaymentMethod = PaymentMethod.Cash,
            CollectionDate = Constants.DefaultDateOnly,
            Description = "Tithe-01",
        };
    }

    public static TitheUpdateDto NewUpdateDto()
    {
        return new TitheUpdateDto()
        {
            MemberId = Constants.DefaultGuid,
            Amount = 100.0m,
            ForYear = 2026,
            ForMonth = MonthOfYear.January,
            PaymentMethod = PaymentMethod.Cash,
            CollectionDate = Constants.DefaultDateOnly,
            Description = "Tithe-01",
        };
    }

    public static TitheUpdateDto NewUpdateDtoWithNullValues()
    {
        return new TitheUpdateDto()
        {
            MemberId = null,
            Amount = null,
            ForYear = null,
            ForMonth = null,
            PaymentMethod = null,
            CollectionDate = null,
            Description = null,
        };
    }
}
