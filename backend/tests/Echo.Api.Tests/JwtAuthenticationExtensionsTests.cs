using System.Security.Claims;
using Echo.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Xunit;

namespace Echo.Api.Tests;

public class JwtAuthenticationExtensionsTests
{
    [Fact]
    public void AddJwtAuthentication_DoesNotMutateDefaultInboundClaimTypeMap()
    {
        var keysBefore = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();
        var (privateKeyB64, publicKeyB64) = JwtTestHelper.GenerateRsaKeyPair();

        var services = new ServiceCollection();
        services.AddJwtAuthentication(JwtTestHelper.BuildConfiguration(privateKeyB64, publicKeyB64));

        var keysAfter = JsonWebTokenHandler.DefaultInboundClaimTypeMap.Keys.ToList();

        Assert.Equal(keysBefore, keysAfter);
    }

    [Fact]
    public async Task ValidatedToken_PreservesRawClaimTypes_WhenMapInboundClaimsIsFalse()
    {
        var (privateKeyB64, publicKeyB64) = JwtTestHelper.GenerateRsaKeyPair();
        var token = JwtTestHelper.CreateSignedToken(privateKeyB64);
        var result = await JwtTestHelper.ValidateTokenAsync(token, publicKeyB64);

        Assert.True(result.IsValid, result.Exception?.Message);

        var principal = new ClaimsPrincipal(result.ClaimsIdentity);

        Assert.Equal("user-id", principal.FindFirstValue("sub"));
        Assert.Equal("Admin", principal.FindFirstValue("role"));
        Assert.Equal("congregation-id", principal.FindFirstValue("congregationId"));
        Assert.Null(principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}
