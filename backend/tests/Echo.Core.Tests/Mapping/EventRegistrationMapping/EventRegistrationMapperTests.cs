using Echo.Core.Mapping.EventRegistrationMapping;
using Echo.Core.Tests.TestData.Factories;

namespace Echo.Core.Tests.Mapping.EventRegistrationMapping;

public class EventRegistrationMapperTests
{
    private readonly IEventRegistrationMapper _mapper = new EventRegistrationMapper();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = EventRegistrationFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.EventId, entity.EventId);
        Assert.Equal(dto.RegistrationDate, entity.RegistrationDate);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = EventRegistrationFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = EventRegistrationFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(dto.MemberId, entity.MemberId);
        Assert.Equal(dto.EventId, entity.EventId);
        Assert.Equal(dto.MemberName, entity.Member.Name);
        Assert.Equal(dto.EventName, entity.Event.Name);
        Assert.Equal(dto.RegistrationDate, entity.RegistrationDate);
        Assert.Equal(dto.CreatedAt, entity.CreatedAt);
    }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = EventRegistrationFactory.NewUpdateDto();

        var entity = EventRegistrationFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.RegistrationDate, entity.RegistrationDate);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = EventRegistrationFactory.NewUpdateDtoWithNullValues();
        var entity = EventRegistrationFactory.NewEntity();

        var original = EventRegistrationFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Id, entity.Id);
        Assert.Equal(original.EventId, entity.EventId);
        Assert.Equal(original.MemberId, entity.MemberId);
        Assert.Equal(original.CongregationId, entity.CongregationId);
        Assert.Equal(original.RegistrationDate, entity.RegistrationDate);
        Assert.Equal(original.CreatedAt, entity.CreatedAt);
        Assert.Equal(original.DeletedAt, entity.DeletedAt);
    }
}
