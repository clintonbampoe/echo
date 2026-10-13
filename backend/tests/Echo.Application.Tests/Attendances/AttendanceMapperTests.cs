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

        Assert.Equal(dto.SessionId, entity.SessionId);
        Assert.Equal(dto.PersonId, entity.PersonId);
        Assert.Equal(dto.CheckedInAt, entity.CheckedInAt);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_Congregation_Session_Person_PersonKindAtTime_RecordedBy_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = AttendanceFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.Congregation);
        Assert.Null(entity.Session);
        Assert.Null(entity.Person);
        Assert.Equal(default, entity.PersonKindAtTime);
        Assert.Null(entity.RecordedBy);
        Assert.Null(entity.Recorder);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = AttendanceFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.SessionId, dto.SessionId);
        Assert.Equal(entity.Session.Date, dto.SessionDate);
        Assert.Equal(entity.Session.Name, dto.SessionName);
        Assert.Equal(entity.Session.SessionLabelId, dto.SessionLabelId);
        Assert.Equal(entity.Session.SessionLabel.Name, dto.SessionLabelName);
        Assert.Equal(entity.PersonId, dto.PersonId);
        Assert.Equal(entity.Person.Name, dto.PersonName);
        Assert.Equal(entity.PersonKindAtTime, dto.PersonKind);
        Assert.Equal(entity.CheckedInAt, dto.CheckedInAt);
        Assert.Equal(entity.RecordedBy, dto.RecordedBy);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void ToDto_ShouldUsePersonKindAtTime_NotCurrentPersonKind()
    {
        // The snapshot must win over the person's current role. A visitor who
        // later became a member still reports as a Visitor for pre-conversion
        // attendance.
        var entity = AttendanceFactory.NewEntity();
        entity.PersonKindAtTime = Echo.Domain.Members.PersonKind.Visitor;
        entity.Person.Kind = Echo.Domain.Members.PersonKind.Member;

        var dto = _mapper.ToDto(entity);

        Assert.Equal(Echo.Domain.Members.PersonKind.Visitor, dto.PersonKind);
    }

    [Fact]
    public void Patch_ShouldUpdateCheckedInAt_WhenDtoHasValue()
    {
        var dto = AttendanceFactory.NewUpdateDto();
        var entity = AttendanceFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.CheckedInAt, entity.CheckedInAt);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = AttendanceFactory.NewUpdateDtoWithNullFields();
        var entity = AttendanceFactory.NewEntity();
        var original = AttendanceFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.CheckedInAt, entity.CheckedInAt);
    }
}
