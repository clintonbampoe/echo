using Echo.Auth.Invitations;
using Microsoft.Extensions.Options;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationCodeProtectorTests
{
    private static InvitationCodeProtector With(byte fill) =>
        new(Options.Create(new InvitationOptions { CodeKey = Convert.ToBase64String(Enumerable.Repeat(fill, 32).ToArray()) }));

    [Fact]
    public void ACodeComesBackAsItWentIn()
    {
        var protector = With(1);

        var stored = protector.Protect("K7QX92MDPW4T");

        Assert.NotEqual("K7QX92MDPW4T", stored);
        Assert.Equal("K7QX92MDPW4T", protector.Unprotect(stored));
    }

    [Fact]
    public void TheSameCodeIsStoredDifferentlyEachTime()
    {
        var protector = With(1);

        Assert.NotEqual(protector.Protect("K7QX92MDPW4T"), protector.Protect("K7QX92MDPW4T"));
    }

    [Fact]
    public void ACodeStoredUnderAnotherKey_CannotBeRead_ButDoesNotThrow()
    {
        var stored = With(1).Protect("K7QX92MDPW4T");

        Assert.Null(With(2).Unprotect(stored));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not base64!")]
    [InlineData("AAAA")]
    public void NothingOrGarbage_GivesNoCode(string? stored)
    {
        Assert.Null(With(1).Unprotect(stored));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not base64!")]
    [InlineData("AAAA")]
    public void AKeyThatIsNot32Bytes_IsRejectedAtStartup(string key)
    {
        Assert.Throws<InvalidOperationException>(() =>
            new InvitationCodeProtector(Options.Create(new InvitationOptions { CodeKey = key }))
        );
    }
}
