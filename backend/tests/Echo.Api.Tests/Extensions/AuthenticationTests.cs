using System.Security.Claims;
using Echo.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class AuthenticationTests
{
    [Fact]
    public void AddJwtAuthentication_DoesNotMutateDefaultInboundClaimTypeMap()
    {
        var keysBefore = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();
        var (privateKeyBase64, publicKeyB64) = AuthenticationTestsHelper.GenerateRsaKeyPair();

        var services = new ServiceCollection();
        services.ConfigureJwtAuthentication(
            AuthenticationTestsHelper.BuildConfiguration(privateKeyBase64, publicKeyB64)
        );

        var keysAfter = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();

        Assert.Equal(keysBefore, keysAfter);
    }

    [Fact]
    public async Task ValidatedToken_PreservesRawClaimTypes_WhenMapInboundClaimsIsFalse()
    {
        var (privateKeyBase64, publicKeyBase64) = AuthenticationTestsHelper.GenerateRsaKeyPair();
        var token = AuthenticationTestsHelper.CreateSignedToken(privateKeyBase64);
        var result = await AuthenticationTestsHelper.ValidateTokenAsync(token, publicKeyBase64);

        Assert.True(result.IsValid, result.Exception?.Message);

        var principal = new ClaimsPrincipal(result.ClaimsIdentity);

        Assert.Equal("user-id", principal.FindFirstValue("sub"));
        Assert.Equal("Admin", principal.FindFirstValue("role"));
        Assert.Equal("congregation-id", principal.FindFirstValue("congregationId"));
        Assert.Null(principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}
