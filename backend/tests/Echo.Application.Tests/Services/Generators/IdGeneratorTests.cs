using Echo.Application.Services.Generators;

namespace Echo.Application.Tests.Services.Generators;

[Trait("Category", "Unit")]
public class IdGeneratorTests
{
    private readonly IIdGenerator _idGenerator = new IdGenerator();

    [Fact]
    public void Generate_WhenCalled_ShouldNotBeEmpty()
    {
        var id = _idGenerator.Generate();

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public void Generate_WhenCalled_ReturnsValidGuid()
    {
        var id = _idGenerator.Generate();

        bool isValid = Guid.TryParse(id.ToString(), out _);
        Assert.True(isValid);
    }

    [Fact]
    public void Generate_WhenCalled_ReturnsVesion7Guid()
    {
        var id = _idGenerator.Generate();

        string idString = id.ToString();
        char versionChar = idString[14];

        Assert.Equal('7', versionChar);
    }
}
