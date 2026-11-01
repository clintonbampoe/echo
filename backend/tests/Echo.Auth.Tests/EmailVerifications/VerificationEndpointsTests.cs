using Echo.Auth.EmailVerifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Tests.EmailVerifications;

[Trait("Category", "Unit")]
public class VerificationEndpointsTests
{
    private static Dictionary<string, RouteEndpoint> Map()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<EmailVerificationService>(); // registered so handler parameters are services
        var app = builder.Build();
        using var instrumentation = new AuthInstrumentation();
        app.MapGroup("/api/v1/auth").MapVerificationEndpoints(instrumentation);

        return ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToDictionary(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName!);
    }

    [Fact]
    public void TheStatusRoute_NeedsASignedInUser()
    {
        var status = Map()["GetVerificationStatus"];

        Assert.Equal("/api/v1/auth/verifications/status", status.RoutePattern.RawText);
        Assert.NotEmpty(status.Metadata.GetOrderedMetadata<IAuthorizeData>());
    }

    [Fact]
    public void TheSendAndVerifyRoutes_StayOpenToPeopleWithoutASession()
    {
        var routes = Map();

        Assert.Empty(routes["SendVerificationLink"].Metadata.GetOrderedMetadata<IAuthorizeData>());
        Assert.Empty(routes["VerifyEmail"].Metadata.GetOrderedMetadata<IAuthorizeData>());
    }
}
