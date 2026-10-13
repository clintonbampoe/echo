using System.Security.Claims;
using Echo.Application.Tithes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Tests.Tithes;

[Trait("Category", "Unit")]
public class TitheAuthorizationTests
{
    private static async Task<bool> IsAllowed(string role, string policy)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options => options.AddFinancePolicies());
        using var provider = services.BuildServiceProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        // Same claim types the JWT validator configures (NameClaimType "sub", RoleClaimType "role").
        var identity = new ClaimsIdentity(
            [new Claim("sub", Guid.NewGuid().ToString()), new Claim("role", role)],
            authenticationType: "test",
            nameType: "sub",
            roleType: "role"
        );

        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(identity), policy);
        return result.Succeeded;
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Accountant", true)]
    [InlineData("Clerk", false)]
    [InlineData("Member", false)]
    public async Task FinanceManage_AllowsOnlyAdminAndAccountant(string role, bool expected)
    {
        Assert.Equal(expected, await IsAllowed(role, FinancePolicies.Manage));
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Accountant", true)]
    [InlineData("Clerk", true)]
    [InlineData("Member", false)]
    public async Task FinanceRecord_AllowsEveryRoleExceptMember(string role, bool expected)
    {
        Assert.Equal(expected, await IsAllowed(role, FinancePolicies.Record));
    }

    [Fact]
    public void EveryTitheRoute_RequiresTheExpectedFinancePolicy()
    {
        var expected = new Dictionary<string, string>
        {
            ["ListTithes"] = FinancePolicies.Manage,
            ["GetTithesByMemberId"] = FinancePolicies.Manage,
            ["GetTitheById"] = FinancePolicies.Manage,
            ["GetTitheSummary"] = FinancePolicies.Manage,
            ["GetTitheMonthlySummary"] = FinancePolicies.Manage,
            ["CreateTithe"] = FinancePolicies.Record,
            ["UpdateTithe"] = FinancePolicies.Manage,
            ["DeleteTithe"] = FinancePolicies.Manage,
        };

        // Handlers take TitheService as a parameter; it must be registered so the endpoint
        // builder treats it as a service rather than an inferred body. Nothing is resolved.
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();
        using var instrumentation = new ApplicationInstrumentation();
        app.MapTitheEndpoints(instrumentation);

        var endpoints = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .ToList();

        var actual = endpoints.ToDictionary(
            endpoint =>
                endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
                ?? endpoint.DisplayName!,
            endpoint =>
                endpoint
                    .Metadata.GetOrderedMetadata<IAuthorizeData>()
                    .Select(data => data.Policy)
                    .Where(policy => policy is not null)
                    .ToList()
        );

        Assert.Equal(expected.Keys.Order(), actual.Keys.Order());
        foreach (var (name, policy) in expected)
            Assert.Equal([policy], actual[name]);
    }
}
