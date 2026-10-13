using Echo.Auth.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Tests.Profile;

[Trait("Category", "Unit")]
public class ProfileEndpointsTests
{
    private static List<RouteEndpoint> Map()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<ProfileService>(); // registered so the handler parameter is a service
        var app = builder.Build();
        using var instrumentation = new AuthInstrumentation();
        app.MapGroup("/api/v1").MapProfileEndpoints(instrumentation);

        return ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();
    }

    [Fact]
    public void ProfileRoutes_AreMappedUnderUsersMe()
    {
        var routes = Map().ToDictionary(
            e => e.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName!,
            e => e.RoutePattern.RawText!.TrimEnd('/')
        );

        Assert.Equal("/api/v1/users/me", routes["GetMyProfile"]);
        Assert.Equal("/api/v1/users/me", routes["UpdateMyProfile"]);
        Assert.Equal(2, routes.Count);
    }

    [Fact]
    public void ProfileRoutes_NeedASignedInUserButNoParticularRole()
    {
        foreach (var endpoint in Map())
        {
            Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
            Assert.DoesNotContain(
                endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>().SelectMany(p => p.Requirements),
                r => r is RolesAuthorizationRequirement
            );
        }
    }

    [Fact]
    public void UpdatingTheProfile_IsRateLimitedLikeTheOtherPasswordRoutes()
    {
        var update = Map().Single(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName == "UpdateMyProfile");

        Assert.Equal("auth", update.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName);
    }
}
