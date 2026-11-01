using Echo.Application.Users;
using Echo.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Tests.Users;

[Trait("Category", "Unit")]
public class UserSecurityTests
{
    private static readonly Guid CongregationA = Guid.Parse("00000000-0000-0000-0000-00000000000a");
    private static readonly Guid CongregationB = Guid.Parse("00000000-0000-0000-0000-00000000000b");

    private static User NewUser(Guid congregationId, Guid id)
    {
        var user = TestData.Shared.CreateUser();
        user.CongregationId = congregationId;
        user.Id = id;
        return user;
    }

    [Fact]
    public void ById_UserOfAnotherCongregation_IsNotReturned()
    {
        var userInB = NewUser(CongregationB, TestData.DefaultId);

        var res = new[] { userInB }
            .AsQueryable()
            .ById(CongregationA, TestData.DefaultId)
            .SingleOrDefault();

        Assert.Null(res);
    }

    [Fact]
    public void ById_UserOfTheSameCongregation_IsReturned()
    {
        var userInA = NewUser(CongregationA, TestData.DefaultId);
        var otherUserInA = NewUser(
            CongregationA,
            Guid.Parse("00000000-0000-0000-0000-000000000002")
        );

        var res = new[] { userInA, otherUserInA }
            .AsQueryable()
            .ById(CongregationA, TestData.DefaultId)
            .SingleOrDefault();

        Assert.Same(userInA, res);
    }

    [Fact]
    public void EveryUserRoute_RequiresTheAdminRole()
    {
        var expectedRoutes = new[]
        {
            "ListUsers",
            "GetUserById",
            "CreateUser",
            "UpdateUser",
            "DeleteUser",
            "SearchUsers",
        };

        // Handlers take UserService as a parameter; it must be registered so the endpoint
        // builder treats it as a service rather than an inferred body. Nothing is resolved.
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();
        using var instrumentation = new ApplicationInstrumentation();
        app.MapUserEndpoints(instrumentation);

        var endpoints = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .ToList();

        var actual = endpoints.ToDictionary(
            endpoint =>
                endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName
                ?? endpoint.DisplayName!,
            endpoint =>
                endpoint
                    .Metadata.GetOrderedMetadata<AuthorizationPolicy>()
                    .SelectMany(policy => policy.Requirements)
                    .OfType<RolesAuthorizationRequirement>()
                    .SelectMany(requirement => requirement.AllowedRoles)
                    .ToList()
        );

        Assert.Equal(expectedRoutes.Order(), actual.Keys.Order());
        foreach (var route in expectedRoutes)
            Assert.Equal([nameof(UserRole.Admin)], actual[route]);
    }
}
