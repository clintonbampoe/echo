using Echo.Core.Dtos;
using Echo.Core.Tests.Helpers;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;

namespace Echo.Core.Tests.TestData.Factories;

public static class AssetFactory
{
    public static Asset NewEntity()
    {
        return new Asset()
        {
            Id = Constants.DefaultGuid,
            CongregationId = Constants.DefaultGuid,
            Congregation = CongregationFactory.NewEntity(),
            Category = AssetCategoryFactory.NewEntity(),
            CategoryId = Constants.DefaultInt,
            Name = "Asset-01",
            SerialNumber = null,
            PurchaseDate = null,
            PurchaseCost = 0,
            CurrentValue = 0,
            Status = default,
            Description = null,
            CreatedAt = Constants.DefaultDateTime,
            DeletedAt = null,
        };
    }

    public static AssetCreateDto NewCreateDto()
    {
        return new AssetCreateDto()
        {
            CategoryId = Constants.DefaultInt,
            Name = "AssetCreate-01",
            SerialNumber = null,
            PurchaseDate = null,
            PurchaseCost = 0,
            CurrentValue = 0,
            Status = default,
            Description = null,
        };
    }

    public static AssetUpdateDto NewUpdateDto()
    {
        return new AssetUpdateDto()
        {
            CategoryId = Constants.DefaultInt,
            Name = "AssetUpdate-01",
            SerialNumber = "SN-01",
            PurchaseDate = new DateOnly(2024, 1, 1),
            PurchaseCost = 500,
            CurrentValue = 400,
            Status = AssetStatus.Active,
            Description = "Updated description",
        };
    }

    public static AssetUpdateDto NewUpdateDtoWithNullFields()
    {
        return new AssetUpdateDto()
        {
            CategoryId = null,
            Name = null,
            SerialNumber = null,
            PurchaseDate = null,
            PurchaseCost = null,
            CurrentValue = null,
            Status = null,
            Description = null,
        };
    }

    public static AssetUpdateDto NewUpdateDtoWithRandomValues()
    {
        return new AssetUpdateDto()
        {
            CategoryId = 99,
            Name = RandomGenerators.String(10),
            SerialNumber = RandomGenerators.String(10),
            PurchaseDate = new DateOnly(2023, 6, 15),
            PurchaseCost = 750,
            CurrentValue = 600,
            Status = AssetStatus.UnderMaintenance,
            Description = RandomGenerators.String(20),
        };
    }
}
