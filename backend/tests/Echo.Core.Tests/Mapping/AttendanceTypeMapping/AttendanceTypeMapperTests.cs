using Echo.Core.Mapping.AttendanceTypeMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AttendanceTypeMapping;

[Trait("Category", "Unit")]
public class AttendanceTypeMapperTests
{
    private readonly AttendanceTypeMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AttendanceTypeFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_AttendanceContext_Member_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AttendanceTypeFactory.NewCreateDto();

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
        var entity = AttendanceTypeFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AttendanceTypeFactory.NewUpdateDto();
        var entity = AttendanceTypeFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AttendanceTypeFactory.NewUpdateDtoWithNullFields();
        var entity = AttendanceTypeFactory.NewEntity();
        var original = AttendanceTypeFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
    }

    [Fact]
    public void ToSearchDto_ShouldMapFields_FromEntityList()
    {
        var entityList = AttendanceTypeFactory.NewEntityList();
        var dtoList = _mapper.ToSearchDto(entityList);

        var firstEntity = entityList.First();
        var firstDto = dtoList.First();

        var lastEntity = entityList.Last();
        var lastDto = dtoList.Last();

        Assert.Equal(firstEntity.Id, firstDto.Id);
        Assert.Equal(firstEntity.Name, firstDto.Name);

        Assert.Equal(lastEntity.Id, lastDto.Id);
        Assert.Equal(lastEntity.Name, lastDto.Name);
    }
}
