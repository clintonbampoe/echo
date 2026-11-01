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

    private static (string Key, int Limit) SearchPartition(HttpContext context)
    {
        var partition = AddRateLimitingExtensions.SearchPartition(context);
        using var limiter = partition.Factory(partition.PartitionKey);
        return (partition.PartitionKey, (int)limiter.GetStatistics()!.CurrentAvailablePermits);
    }

    [Fact]
    public void Search_CountsEachSignedInPersonOnTheirOwn_SoAnOfficeDoesNotShareOneAllowance()
    {
        var first = SearchPartition(Request("203.0.113.5", "user-1"));
        var second = SearchPartition(Request("203.0.113.5", "user-2"));

        Assert.Equal("user:user-1", first.Key);
        Assert.NotEqual(first.Key, second.Key);
        Assert.Equal(AddRateLimitingExtensions.SearchPerMinute, first.Limit);
    }

    [Fact]
    public void Search_HasTheOrdinaryAllowance_NotATighterOne()
    {
        Assert.Equal(
            AddRateLimitingExtensions.SignedInPerMinute,
            AddRateLimitingExtensions.SearchPerMinute
        );
    }

    [Fact]
    public void Search_CountsAnonymousCallersByAddress()
    {
        Assert.Equal("ip:203.0.113.5", SearchPartition(Request("203.0.113.5")).Key);
    }

    [Theory]
    [InlineData("login", AddRateLimitingExtensions.LoginPerMinute)]
    [InlineData("session", AddRateLimitingExtensions.SessionPerMinute)]
    [InlineData("auth", AddRateLimitingExtensions.AuthPerMinute)]
    public void TheSignInPolicies_CountByAddress_EachWithItsOwnAllowance(
        string policy,
        int perMinute
    )
    {
        var partition = AddRateLimitingExtensions.AddressPartition(
            Request("203.0.113.5"),
            perMinute
        );
        using var limiter = partition.Factory(partition.PartitionKey);

        Assert.Equal("203.0.113.5", partition.PartitionKey);
        Assert.Equal(perMinute, (int)limiter.GetStatistics()!.CurrentAvailablePermits);
        Assert.True(perMinute > 0, policy);
    }

    [Fact]
    public void TheAllowances_AreOrderedByHowMuchThereIsToGuess()
    {
        Assert.True(
            AddRateLimitingExtensions.AuthPerMinute < AddRateLimitingExtensions.LoginPerMinute
        );
        Assert.True(
            AddRateLimitingExtensions.LoginPerMinute < AddRateLimitingExtensions.SessionPerMinute
        );
    }
}
