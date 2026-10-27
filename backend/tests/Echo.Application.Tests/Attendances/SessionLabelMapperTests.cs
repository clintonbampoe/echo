using AutoFixture;
using Echo.Application.Attendances;
using Echo.Domain.Attendances;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class SessionLabelMapperTests
{
    private readonly ISessionLabelMapper _mapper = new SessionLabelMapper();

    private static SessionLabel CreateSessionLabel(Fixture fixture) =>
        fixture
            .Build<SessionLabel>()
            .With(l => l.Name, "Sunday Service")
            .With(l => l.Category, SessionCategory.Service)
            .Create();

    [Fact]
    public Task ToDto_ShouldMapAllFields_FromEntity()
    {
        var entity = CreateSessionLabel(TestData.NewFixture());

        return Verify(_mapper.ToDto(entity));
    }

    [Fact]
    public Task ToEntity_ShouldMapAllFields_FromCreateDto()
    {
        var dto = TestData
            .NewFixture()
            .Build<SessionLabelCreateDto>()
            .With(d => d.Name, "Sunday Service")
            .With(d => d.Category, SessionCategory.Service)
            .Create();

        return Verify(_mapper.ToEntity(dto));
    }

    [Fact]
    public Task ToListDto_ShouldMapAllFields_FromEntityList()
    {
        var fixture = TestData.NewFixture();
        var entities = TestData.Many(3, () => CreateSessionLabel(fixture));

        return Verify(_mapper.ToListDto(entities));
    }

    [Fact]
    public Task ToSearchDto_ShouldMapIdNameAndCategory_FromEntityList()
    {
        var fixture = TestData.NewFixture();
        var entities = TestData.Many(3, () => CreateSessionLabel(fixture));

        return Verify(_mapper.ToSearchDto(entities));
    }

    [Fact]
    public Task Patch_ShouldApplyAllFieldsInDto_ToEntity()
    {
        var dto = TestData
            .NewFixture()
            .Build<SessionLabelUpdateDto>()
            .With(d => d.Name, "Midweek Service")
            .Create();
        var entity = CreateSessionLabel(TestData.NewFixture());

        _mapper.Patch(dto, entity);

        return Verify(entity);
    }

    [Fact]
    public Task Patch_ShouldIgnoreAllNullFieldsInDto()
    {
        // Every member of SessionLabelUpdateDto is nullable, so the default instance is all-null.
        var nullDto = new SessionLabelUpdateDto();
        var entity = CreateSessionLabel(TestData.NewFixture());

        _mapper.Patch(nullDto, entity);

        return Verify(entity);
    }
}
