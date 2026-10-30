using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Echo.Api.Extensions;
using Microsoft.AspNetCore.Http;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class RateLimitPartitionTests
{
    private static DefaultHttpContext Request(string ip, string? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        if (userId is not null)
            context.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId)], "test")
            );
        return context;
    }

    private static (string Key, int Limit) Partition(HttpContext context)
    {
        var partition = AddRateLimitingExtensions.GlobalPartition(context);
        using var limiter = partition.Factory(partition.PartitionKey);
        // A fresh limiter has all of its allowance left.
        return (partition.PartitionKey, (int)limiter.GetStatistics()!.CurrentAvailablePermits);
    }

    [Fact]
    public void ASignedInPerson_IsCountedOnTheirOwn_WithTheLargerAllowance()
    {
        var (key, limit) = Partition(Request("203.0.113.5", "user-1"));

        Assert.Equal("user:user-1", key);
        Assert.Equal(AddRateLimitingExtensions.SignedInPerMinute, limit);
    }

    [Fact]
    public void TwoPeopleBehindOneAddress_DoNotShareAnAllowance()
    {
        var first = Partition(Request("203.0.113.5", "user-1")).Key;
        var second = Partition(Request("203.0.113.5", "user-2")).Key;

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void SomeoneNotSignedIn_IsCountedByAddress_WithTheSmallerAllowance()
    {
        var (key, limit) = Partition(Request("203.0.113.5"));

        Assert.Equal("ip:203.0.113.5", key);
        Assert.Equal(AddRateLimitingExtensions.AnonymousPerMinute, limit);
    }
}
