using Echo.Application.Attendances;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionMapperTests
{
    private readonly SessionMapper _mapper = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = SessionFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(dto.SessionLabelId, entity.SessionLabelId);
        Assert.Equal(dto.Date, entity.Date);
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.Topic, entity.Topic);
        Assert.Equal(dto.Notes, entity.Notes);
        Assert.Equal(dto.StartsAt, entity.StartsAt);
        Assert.Equal(dto.EndsAt, entity.EndsAt);
    }

    [Fact]
    public void ToEntity_ShouldNotMap_Id_CongregationId_SessionLabel_Status_CreatedBy_CreatedAt_DeletedAt_FromCreateDto()
    {
        var dto = SessionFactory.NewCreateDto();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(Guid.Empty, entity.Id);
        Assert.Equal(Guid.Empty, entity.CongregationId);
        Assert.Null(entity.SessionLabel);
        Assert.Equal(default, entity.Status);
        Assert.Null(entity.CreatedBy);
        Assert.Null(entity.Creator);
        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = SessionFactory.NewEntity();

        var dto = _mapper.ToDto(entity);

        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.SessionLabelId, dto.SessionLabelId);
        Assert.Equal(entity.SessionLabel.Name, dto.SessionLabelName);
        Assert.Equal(entity.SessionLabel.Category, dto.Category);
        Assert.Equal(entity.Date, dto.Date);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.Topic, dto.Topic);
        Assert.Equal(entity.Notes, dto.Notes);
        Assert.Equal(entity.StartsAt, dto.StartsAt);
        Assert.Equal(entity.EndsAt, dto.EndsAt);
        Assert.Equal(entity.Status, dto.Status);
        Assert.Equal(entity.CreatedBy, dto.CreatedBy);
        Assert.Null(dto.CreatedByName);
        Assert.Equal(entity.CreatedAt, dto.CreatedAt);
        Assert.Equal(0, dto.AttendanceCount);
    }

    //     [Fact]
    //     public void ToDto_ShouldMapCreatorName_WhenCreatorIsSet()
    //     {
    //         var entity = SessionFactory.NewEntity();
    //         entity.CreatedBy = Constants.DefaultGuid;
    //         entity.Creator = new Domain.Users.User { Name = "Admin User" };
    //
    //         var dto = _mapper.ToDto(entity);
    //
    //         Assert.Equal("Admin User", dto.CreatedByName);
    //     }

    [Fact]
    public void Patch_ShouldUpdateAllFields_WhenDtoHasValues()
    {
        var dto = SessionFactory.NewUpdateDto();
        var entity = SessionFactory.NewEntity();

        _mapper.Patch(dto, entity);

        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.Topic, entity.Topic);
        Assert.Equal(dto.Notes, entity.Notes);
        Assert.Equal(dto.StartsAt, entity.StartsAt);
        Assert.Equal(dto.EndsAt, entity.EndsAt);
        Assert.Equal(dto.Status, entity.Status);
    }

    [Fact]
    public void Patch_ShouldPreserveAllEntityFields_WhenDtoFieldsAreNull()
    {
        var nullDto = SessionFactory.NewUpdateDtoWithNullFields();
        var entity = SessionFactory.NewEntity();
        var original = SessionFactory.NewEntity();

        _mapper.Patch(nullDto, entity);

        Assert.Equal(original.Name, entity.Name);
        Assert.Equal(original.Topic, entity.Topic);
        Assert.Equal(original.Notes, entity.Notes);
        Assert.Equal(original.StartsAt, entity.StartsAt);
        Assert.Equal(original.EndsAt, entity.EndsAt);
        Assert.Equal(original.Status, entity.Status);
    }

    [Fact]
    public void Patch_ShouldNotChangeDate_Or_SessionLabelId()
    {
        // Both are immutable. The update DTO does not carry them, so Patch
        // must not touch them even when the entity has non-default values.
        var entity = SessionFactory.NewEntity();
        var originalDate = entity.Date;
        var originalLabelId = entity.SessionLabelId;

        _mapper.Patch(SessionFactory.NewUpdateDto(), entity);

        Assert.Equal(originalDate, entity.Date);
        Assert.Equal(originalLabelId, entity.SessionLabelId);
    }
}
