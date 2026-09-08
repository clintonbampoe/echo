using Echo.Core.Mapping.AssetMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AssetMapping;

[Trait("Category", "Unit")]
public class AssetMapperTests
{
    private readonly AssetMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AssetFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.CategoryId, entity.CategoryId);
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.SerialNumber, entity.SerialNumber);
        Assert.Equal(dto.PurchaseDate, entity.PurchaseDate);
        Assert.Equal(dto.PurchaseCost, entity.PurchaseCost);
        Assert.Equal(dto.CurrentValue, entity.CurrentValue);
        Assert.Equal(dto.Status, entity.Status);
        Assert.Equal(dto.Description, entity.Description);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_Category_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AssetFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Null(entity.Category);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AssetFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.CategoryId, dto.CategoryId);
        Assert.Equal(entity.Category.Name, dto.CategoryName);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.SerialNumber, dto.SerialNumber);
        Assert.Equal(entity.PurchaseDate, dto.PurchaseDate);
        Assert.Equal(entity.PurchaseCost, dto.PurchaseCost);
        Assert.Equal(entity.CurrentValue, dto.CurrentValue);
        Assert.Equal(entity.Status, dto.Status);
        Assert.Equal(entity.Description, dto.Description);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AssetFactory.NewUpdateDtoWithRandomValues();
        var entity = AssetFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.CategoryId, entity.CategoryId);
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.SerialNumber, entity.SerialNumber);
        Assert.Equal(dto.PurchaseDate, entity.PurchaseDate);
        Assert.Equal(dto.PurchaseCost, entity.PurchaseCost);
        Assert.Equal(dto.CurrentValue, entity.CurrentValue);
        Assert.Equal(dto.Status, entity.Status);
        Assert.Equal(dto.Description, entity.Description);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AssetFactory.NewUpdateDtoWithNullFields();
        var entity = AssetFactory.NewEntity();
        var original = AssetFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.CategoryId, entity.CategoryId);
        Assert.Equal(original.Name, entity.Name);
        Assert.Equal(original.SerialNumber, entity.SerialNumber);
        Assert.Equal(original.PurchaseDate, entity.PurchaseDate);
        Assert.Equal(original.PurchaseCost, entity.PurchaseCost);
        Assert.Equal(original.CurrentValue, entity.CurrentValue);
        Assert.Equal(original.Status, entity.Status);
        Assert.Equal(original.Description, entity.Description);
    }
}
