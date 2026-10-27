using Echo.Application.Congregations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Tests.Congregations;

[Trait("Category", "Unit")]
public class CongregationEndpointTests
{
    [Fact]
    public void GetCurrentCongregation_IsMappedAtSlashCongregation_ForAnySignedInUser()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();
        using var instrumentation = new ApplicationInstrumentation();
        app.MapCongregationEndpoints(instrumentation);

        var endpoints = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();
        var endpoint = Assert.Single(endpoints);

        Assert.Equal(
            "GetCurrentCongregation",
            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
        );
        Assert.Equal("/congregation", endpoint.RoutePattern.RawText!.TrimEnd('/'));
        Assert.Contains("GET", endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
        Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
        Assert.DoesNotContain(
            endpoint
                .Metadata.GetOrderedMetadata<AuthorizationPolicy>()
                .SelectMany(p => p.Requirements),
            r => r is RolesAuthorizationRequirement
        );
    }
}
