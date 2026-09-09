using Echo.Core.Mapping.AssetCategoryMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AssetCategoryMapping;

[Trait("Category", "Unit")]
public class AssetCategoryMapperTests
{
    private readonly AssetCategoryMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AssetCategoryFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AssetCategoryFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(0, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AssetCategoryFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AssetCategoryFactory.NewUpdateDtoWithRandomValues();
        var entity = AssetCategoryFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AssetCategoryFactory.NewUpdateDtoWithNullFields();
        var entity = AssetCategoryFactory.NewEntity();
        var original = AssetCategoryFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
    }
}
