namespace Echo.Shared.Tests.Query;

[Trait("Category", "Unit")]
public class LikeEscapeTests
{
    [Theory]
    [InlineData("john", "john")]
    [InlineData("%", "\\%")]
    [InlineData("_", "\\_")]
    [InlineData("50%_off", "50\\%\\_off")]
    [InlineData("a\\b", "a\\\\b")]
    public void EscapeLike_EscapesWildcardsAndBackslash(string input, string expected)
    {
        Assert.Equal(expected, Echo.Shared.Query.Extensions.EscapeLike(input));
    }
}
