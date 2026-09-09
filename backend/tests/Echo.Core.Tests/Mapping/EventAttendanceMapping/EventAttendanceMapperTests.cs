using Echo.Core.Mapping.EventAttendanceMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.EventAttendanceMapping;

public class EventAttendanceMapperTests
{
    private readonly IEventAttendanceMapper _mapper = new EventAttendanceMapper();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = EventAttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.EventId, entity.EventId);
        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = EventAttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = EventAttendanceFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.EventId, entity.EventId);
        Assert.Equal(dto.MemberName, entity.Member.Name);
        Assert.Equal(dto.EventName, entity.Event.Name);
        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
        Assert.Equal(dto.CreatedAt, entity.CreatedAt);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = EventAttendanceFactory.NewUpdateDto();

        var entity = EventAttendanceFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.CheckInTime, entity.CheckInTime);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = EventAttendanceFactory.NewUpdateDtoWithNullValues();
        var entity = EventAttendanceFactory.NewEntity();

        var original = EventAttendanceFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Id, entity.Id);
        Assert.Equal(original.EventId, entity.EventId);
        Assert.Equal(original.MemberId, entity.MemberId);
        Assert.Equal(original.CongregationId, entity.CongregationId);
        Assert.Equal(original.CheckInTime, entity.CheckInTime);
        Assert.Equal(original.CreatedAt, entity.CreatedAt);
        Assert.Equal(original.DeletedAt, entity.DeletedAt);
    }
}
