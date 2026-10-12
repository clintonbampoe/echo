using Echo.Application.Attendances;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionLabelMapperTests
{
    private readonly SessionLabelMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = SessionLabelFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.Category, entity.Category);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = SessionLabelFactory.NewCreateDto();

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
        var entity = SessionLabelFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.Category, dto.Category);
    }

    [Fact]
    public void Patch_ShouldUpdateName_WhenDtoHasValue()
    {
        var dto = SessionLabelFactory.NewUpdateDto();
        var entity = SessionLabelFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void Patch_ShouldPreserveEntityName_WhenDtoNameIsNull()
    {
        var nullDto = SessionLabelFactory.NewUpdateDtoWithNullFields();
        var entity = SessionLabelFactory.NewEntity();
        var original = SessionLabelFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
    }

    [Fact]
    public void Patch_ShouldNotChangeCategory_EvenWhenEntityHasDifferentCategory()
    {
        // Category is set at creation and immutable thereafter. Patch must
        // not touch it — the update DTO does not even carry the field.
        var entity = SessionLabelFactory.NewEntity();
        entity.Category = SessionCategory.Service;

        _mapper.Patch(SessionLabelFactory.NewUpdateDto(), entity);

        Assert.Equal(SessionCategory.Service, entity.Category);
    }

    [Fact]
    public void ToSearchDto_ShouldMapFields_FromEntityList()
    {
        var entityList = SessionLabelFactory.NewEntityList();
        var dtoList = _mapper.ToSearchDto(entityList);

        var firstEntity = entityList.First();
        var firstDto = dtoList.First();

        var lastEntity = entityList.Last();
        var lastDto = dtoList.Last();

        Assert.Equal(firstEntity.Id, firstDto.Id);
        Assert.Equal(firstEntity.Name, firstDto.Name);
        Assert.Equal(firstEntity.Category, firstDto.Category);

        Assert.Equal(lastEntity.Id, lastDto.Id);
        Assert.Equal(lastEntity.Name, lastDto.Name);
        Assert.Equal(lastEntity.Category, lastDto.Category);
    }
}
