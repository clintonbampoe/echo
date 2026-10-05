using Echo.Application.Attendances;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class AttendanceMapperTests
{
    private readonly AttendanceMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.AttendanceTypeId, entity.AttendanceTypeId);
        Assert.Equal(dto.PersonId, entity.PersonId);
        Assert.Equal(dto.Date, entity.Date);
        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
        Assert.Equal(dto.Notes, entity.Notes);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_AttendanceType_Person_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Null(entity.AttendanceType);
        Assert.Null(entity.Person);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AttendanceFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.AttendanceTypeId, dto.AttendanceTypeId);
        Assert.Equal(entity.AttendanceType.Name, dto.AttendanceTypeName);
        Assert.Equal(entity.PersonId, dto.PersonId);
        Assert.Equal(entity.Person.Name, dto.PersonName);
        Assert.Equal(entity.Person.Kind, dto.PersonKind);
        Assert.Equal(entity.Date, dto.Date);
        Assert.Equal(entity.CheckInTime, dto.CheckInTime);
        Assert.Equal(entity.Notes, dto.Notes);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AttendanceFactory.NewUpdateDto();
        var entity = AttendanceFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
        Assert.Equal(dto.Notes, entity.Notes);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AttendanceFactory.NewUpdateDtoWithNullFields();
        var entity = AttendanceFactory.NewEntity();
        var original = AttendanceFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.CheckInTime, entity.CheckInTime);
        Assert.Equal(original.Notes, entity.Notes);
    }
}
