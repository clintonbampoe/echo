using AutoFixture;
using Echo.Application.Attendances;
using Echo.Domain.Attendances;
using Echo.Domain.Members;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class AttendanceMapperTests
{
    private readonly IAttendanceMapper _mapper = new AttendanceMapper();

    private Attendance CreateAttendance(Fixture fixture)
    {
        var sessionLabel = fixture
            .Build<SessionLabel>()
            .With(l => l.Name, "Sunday Service")
            .Create();
        var session = fixture
            .Build<Session>()
            .With(s => s.Name, "Main Service")
            .With(s => s.SessionLabel, sessionLabel)
            .Create();
        var person = fixture.Build<Person>().With(p => p.Name, "Jane Doe").Create();

        return fixture
            .Build<Attendance>()
            .With(a => a.Session, session)
            .With(a => a.Person, person)
            .Create();
    }

    [Fact]
    public Task ToDto_ShouldMapAllFieldsAndFlattenSessionLabelAndPersonNames_FromEntity()
    {
        var entity = CreateAttendance(TestData.NewFixture());

        return Verify(_mapper.ToDto(entity));
    }

    [Fact]
    public Task ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = TestData
            .NewFixture()
            .Build<AttendanceCreateDto>()
            .With(d => d.CheckedInAt, new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc))
            .Create();

        return Verify(_mapper.ToEntity(dto));
    }

    [Fact]
    public Task ToEntity_ShouldDefaultCheckedInAt_WhenDtoOmitsIt()
    {
        // The service supplies the current time when the request has none.
        var dto = TestData
            .NewFixture()
            .Build<AttendanceCreateDto>()
            .With(d => d.CheckedInAt, (DateTime?)null)
            .Create();

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(default, entity.CheckedInAt);

        return Verify(entity);
    }

    [Fact]
    public Task ToListDto_ShouldMapAllFields_FromEntityList()
    {
        var fixture = TestData.NewFixture();
        var entities = TestData.Many(3, () => CreateAttendance(fixture));

        return Verify(_mapper.ToListDto(entities));
    }

    [Fact]
    public Task Patch_ShouldApplyAllFieldsInDto_ToEntity()
    {
        var dto = TestData
            .NewFixture()
            .Build<AttendanceUpdateDto>()
            .With(d => d.CheckedInAt, new DateTime(2026, 4, 2, 10, 30, 0, DateTimeKind.Utc))
            .Create();
        var entity = CreateAttendance(TestData.NewFixture());

        _mapper.Patch(dto, entity);

        return Verify(entity);
    }

    [Fact]
    public Task Patch_ShouldIgnoreAllNullFieldsInDto()
    {
        // Every member of AttendanceUpdateDto is nullable, so the default instance is all-null.
        var nullDto = new AttendanceUpdateDto();
        var entity = CreateAttendance(TestData.NewFixture());

        _mapper.Patch(nullDto, entity);

        return Verify(entity);
    }
}
