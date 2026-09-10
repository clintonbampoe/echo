using Echo.Core.Mapping.AttendanceContextMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AttendanceContextMapping;

[Trait("Category", "Unit")]
public class AttendanceContextMapperTests
{
    private readonly AttendanceContextMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AttendanceContextFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.AttendanceTypeId, entity.AttendanceTypeId);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_AttendanceType_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AttendanceContextFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(0, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Null(entity.AttendanceType);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AttendanceContextFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.AttendanceType.Name, dto.AttendanceTypeName);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AttendanceContextFactory.NewUpdateDto();
        var entity = AttendanceContextFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AttendanceContextFactory.NewUpdateDtoWithNullFields();
        var entity = AttendanceContextFactory.NewEntity();
        var original = AttendanceContextFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
    }

    [Fact]
    public void ToSearchDto_ShouldMapFields_FromEntityList()
    {
        var entityList = AttendanceContextFactory.NewEntityList();
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
