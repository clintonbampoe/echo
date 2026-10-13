using System.Net;
using System.Text.Json;
using Echo.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class HealthEndpointsTests
{
    private static async Task<WebApplication> Start(HealthStatus readinessCheck)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization(options =>
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build()
        );
        builder
            .Services.AddHealthChecks()
            .AddCheck(
                "database",
                () => new HealthCheckResult(readinessCheck),
                tags: [Health.HealthCheckTags.Readiness]
            );

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthEndpoints();
        await app.StartAsync();
        return app;
    }

    private static async Task<(HttpStatusCode, JsonElement)> Get(WebApplication app, string url)
    {
        using var client = app.GetTestClient();
        var res = await client.GetAsync(url, TestContext.Current.CancellationToken);
        var json = await res.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (res.StatusCode, JsonDocument.Parse(json).RootElement);
    }

    [Fact]
    public async Task Live_IsAnonymousAndIgnoresDependencies()
    {
        await using var app = await Start(HealthStatus.Unhealthy);

        var (status, body) = await Get(app, "/api/health/live");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("Healthy", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Ready_HealthyDependencies_Returns200()
    {
        await using var app = await Start(HealthStatus.Healthy);

        var (status, body) = await Get(app, "/api/health/ready");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal("database", body.GetProperty("checks")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task Ready_UnhealthyDependency_Returns503()
    {
        await using var app = await Start(HealthStatus.Unhealthy);

        var (status, body) = await Get(app, "/api/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal("Unhealthy", body.GetProperty("status").GetString());
    }
}
