using System.Security.Claims;
using Echo.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Echo.Api.Tests;

public class JwtAuthenticationExtensionsTests
{
    [Fact]
    public void AddJwtAuthentication_DoesNotMutateDefaultInboundClaimTypeMap()
    {
        var keysBefore = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();
        var (privateKeyBase64, publicKeyB64) = JwtTestHelper.GenerateRsaKeyPair();

        var services = new ServiceCollection();
        services.AddJwtAuthentication(
            JwtTestHelper.BuildConfiguration(privateKeyBase64, publicKeyB64)
        );

        var keysAfter = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();

        Assert.Equal(keysBefore, keysAfter);
    }

    [Fact]
    public async Task ValidatedToken_PreservesRawClaimTypes_WhenMapInboundClaimsIsFalse()
    {
        var (privateKeyBase64, publicKeyBase64) = JwtTestHelper.GenerateRsaKeyPair();
        var token = JwtTestHelper.CreateSignedToken(privateKeyBase64);
        var result = await JwtTestHelper.ValidateTokenAsync(token, publicKeyBase64);

        Assert.True(result.IsValid, result.Exception?.Message);

        var principal = new ClaimsPrincipal(result.ClaimsIdentity);

        Assert.Equal("user-id", principal.FindFirstValue("sub"));
        Assert.Equal("Admin", principal.FindFirstValue("role"));
        Assert.Equal("congregation-id", principal.FindFirstValue("congregationId"));
        Assert.Null(principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}
