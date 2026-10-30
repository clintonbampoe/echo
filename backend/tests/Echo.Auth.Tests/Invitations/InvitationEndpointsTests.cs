using Echo.Auth.Invitations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Tests.Invitations;

[Trait("Category", "Unit")]
public class InvitationEndpointsTests
{
    private static Dictionary<string, RouteEndpoint> Map()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<InvitationService>(); // registered so handler parameters are services
        var app = builder.Build();
        using var instrumentation = new AuthInstrumentation();
        app.MapGroup("/api/v1/auth").MapInvitationEndpoints(instrumentation);

        return ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToDictionary(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName!);
    }

    private static string Verb(RouteEndpoint e) =>
        e.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods.Single();

    [Fact]
    public void TheManagementRoutes_AreMappedWithTheExpectedVerbsAndPaths()
    {
        var routes = Map();

        Assert.Equal(
            new[]
            {
                ("CreateInvitation", "POST", "/api/v1/auth/invitations"),
                ("ListInvitations", "GET", "/api/v1/auth/invitations"),
                ("InviteByEmail", "POST", "/api/v1/auth/invitations/email"),
                ("GetInvitationSummary", "GET", "/api/v1/auth/invitations/summary"),
                ("GetInvitation", "GET", "/api/v1/auth/invitations/{id:guid}"),
                ("UpdateInvitation", "PUT", "/api/v1/auth/invitations/{id:guid}"),
                ("RevokeInvitation", "POST", "/api/v1/auth/invitations/{id:guid}/revoke"),
                ("DeleteInvitation", "DELETE", "/api/v1/auth/invitations/{id:guid}"),
                ("LookupInvitation", "GET", "/api/v1/auth/invitations/{code}"),
            }.Order(),
            routes.Select(r => (r.Key, Verb(r.Value), r.Value.RoutePattern.RawText!.TrimEnd('/'))).Order()
        );
    }

    [Fact]
    public void EveryManagementRoute_IsAdminOnly()
    {
        foreach (var (name, endpoint) in Map().Where(r => r.Key != "LookupInvitation"))
        {
            var roles = endpoint
                .Metadata.GetOrderedMetadata<AuthorizationPolicy>()
                .SelectMany(p => p.Requirements)
                .OfType<RolesAuthorizationRequirement>()
                .SelectMany(r => r.AllowedRoles)
                .ToList();

            Assert.True(roles.SequenceEqual(["Admin"]), $"{name} must require the Admin role");
            Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        }
    }

    [Fact]
    public void TheLookup_IsAnonymousButRateLimited()
    {
        var lookup = Map()["LookupInvitation"];

        Assert.NotNull(lookup.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Equal("auth", lookup.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName);
    }
}
