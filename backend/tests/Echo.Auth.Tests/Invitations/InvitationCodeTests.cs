using System.Text.RegularExpressions;
using Echo.Auth.Invitations;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationCodeTests
{
    [Fact]
    public void Generate_ReturnsTwelveUnambiguousCharactersInThreeGroups()
    {
        var code = InvitationCode.Generate();

        // Crockford base32 without I, L, O and U.
        Assert.Matches(new Regex("^[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}$"), code);
    }

    [Fact]
    public void Generate_ReturnsADifferentCodeEachTime()
    {
        var codes = Enumerable.Range(0, 200).Select(_ => InvitationCode.Generate()).ToHashSet();

        Assert.Equal(200, codes.Count);
    }

    [Fact]
    public void Normalize_OfAGeneratedCode_GivesTheTwelveCharacterForm()
    {
        var code = InvitationCode.Generate();

        var normalized = InvitationCode.Normalize(code);

        Assert.Equal(code.Replace("-", ""), normalized);
    }

    [Theory]
    [InlineData("k7qx-92md-pw4t", "K7QX92MDPW4T")]
    [InlineData("  K7QX 92MD PW4T  ", "K7QX92MDPW4T")]
    [InlineData("K7QX92MDPW4T", "K7QX92MDPW4T")]
    [InlineData("K7QX-92MD-PW4T\n", "K7QX92MDPW4T")]
    public void Normalize_IgnoresCaseDashesAndSpaces(string input, string expected)
    {
        Assert.Equal(expected, InvitationCode.Normalize(input));
    }

    [Fact]
    public void Normalize_ReadsLookAlikesTheWayTheyWereMeant_OAsZeroAndIAndLAsOne()
    {
        // 0 and 1 are in the alphabet; o, i and l are what people mistype for them.
        Assert.Equal("0123456789AB", InvitationCode.Normalize("Ol23-4567-89ab"));
        Assert.Equal("111111111111", InvitationCode.Normalize("iIlL-iIlL-iIlL"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("K7QX-92MD-PW4")] // too short
    [InlineData("K7QX-92MD-PW4TA")] // too long
    [InlineData("K7QX-92MD-PW4U")] // U is not in the alphabet
    [InlineData("K7QX-92MD-PW4!")]
    [InlineData("aB3dE_9xK-Q")] // an old-format token
    public void Normalize_ReturnsNull_WhenTheInputCannotBeACode(string? input)
    {
        Assert.Null(InvitationCode.Normalize(input));
    }
}
