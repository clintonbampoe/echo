using Echo.Core.Mapping.AttendanceMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.AttendanceMapping;

[Trait("Category", "Unit")]
public class AttendanceMapperTests
{
    private readonly AttendanceMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = AttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.AttendanceContextId, entity.AttendanceContextId);
        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.AttendeeType, entity.AttendeeType);
        Assert.Equal(dto.ForDate, entity.ForDate);
        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
        Assert.Equal(dto.Description, entity.Description);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_AttendanceContext_Member_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Null(entity.AttendanceContext);
        Assert.Null(entity.Member);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AttendanceFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.AttendanceContextId, dto.AttendanceContextId);
        Assert.Equal(entity.AttendanceContext.Name, dto.AttendanceContextName);
        Assert.Equal(entity.AttendanceContext.AttendanceType.Name, dto.AttendanceTypeName);
        Assert.Equal(entity.MemberId, dto.MemberId);
        Assert.Equal(entity.AttendeeType, dto.AttendeeType);
        Assert.Equal(entity.ForDate, dto.ForDate);
        Assert.Equal(entity.CheckInTime, dto.CheckInTime);
        Assert.Equal(entity.Description, dto.Description);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = AttendanceFactory.NewUpdateDto();
        var entity = AttendanceFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.AttendanceContextId, entity.AttendanceContextId);
        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.AttendeeType, entity.AttendeeType);
        Assert.Equal(dto.ForDate, entity.ForDate);
        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
        Assert.Equal(dto.Description, entity.Description);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AttendanceFactory.NewUpdateDtoWithNullFields();
        var entity = AttendanceFactory.NewEntity();
        var original = AttendanceFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.AttendanceContextId, entity.AttendanceContextId);
        Assert.Equal(original.MemberId, entity.MemberId);
        Assert.Equal(original.AttendeeType, entity.AttendeeType);
        Assert.Equal(original.ForDate, entity.ForDate);
        Assert.Equal(original.CheckInTime, entity.CheckInTime);
        Assert.Equal(original.Description, entity.Description);
    }
}
