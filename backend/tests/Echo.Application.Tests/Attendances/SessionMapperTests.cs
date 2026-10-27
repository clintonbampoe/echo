using AutoFixture;
using Echo.Application.Attendances;
using Echo.Domain.Attendances;
using Echo.Domain.Users;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionMapperTests
{
    private readonly ISessionMapper _mapper = new SessionMapper();

    private Session CreateSession(Fixture fixture)
    {
        var sessionLabel = fixture
            .Build<SessionLabel>()
            .With(l => l.Name, "Sunday Service")
            .Create();
        var creator = fixture.Build<User>().With(u => u.Name, "Jane Doe").Create();

        return fixture
            .Build<Session>()
            .With(s => s.SessionLabel, sessionLabel)
            .With(s => s.Creator, creator)
            .Create();
    }

    [Fact]
    public Task ToDto_ShouldMapAllFieldsAndFlattenSessionLabelAndCreatorNames_FromEntity()
    {
        var entity = CreateSession(TestData.NewFixture());

        return Verify(_mapper.ToDto(entity));
    }

    [Fact]
    public Task ToDto_ShouldLeaveAttendanceCountAtZero()
    {
        // AttendanceCount is populated by the service after a batched count query.
        var entity = CreateSession(TestData.NewFixture());

        var dto = _mapper.ToDto(entity);

        Assert.Equal(0, dto.AttendanceCount);

        return Verify(dto);
    }

    [Fact]
    public Task ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = TestData
            .NewFixture()
            .Build<SessionCreateDto>()
            .With(d => d.SessionLabelId, 3)
            .With(d => d.Name, "Main Service")
            .Create();

        return Verify(_mapper.ToEntity(dto));
    }

    [Fact]
    public Task ToListDto_ShouldMapAllFields_FromEntityList()
    {
        var fixture = TestData.NewFixture();
        var entities = TestData.Many(3, () => CreateSession(fixture));

        return Verify(_mapper.ToListDto(entities));
    }

    [Fact]
    public Task Patch_ShouldApplyAllFieldsInDto_ToEntity()
    {
        var dto = TestData
            .NewFixture()
            .Build<SessionUpdateDto>()
            .With(d => d.Name, "Midweek Service")
            .With(d => d.Status, SessionStatus.Completed)
            .Create();
        var entity = CreateSession(TestData.NewFixture());

        _mapper.Patch(dto, entity);

        return Verify(entity);
    }

    [Fact]
    public Task Patch_ShouldIgnoreAllNullFieldsInDto()
    {
        // Every member of SessionUpdateDto is nullable, so the default instance is all-null.
        var nullDto = new SessionUpdateDto();
        var entity = CreateSession(TestData.NewFixture());

        _mapper.Patch(nullDto, entity);

        return Verify(entity);
    }
}
