using System.Reflection;
using Echo.Application;
using Echo.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Api.Tests.Extensions;

// A frontend developer should never have to guess what a parameter does. This reads every endpoint
// the API maps and fails when a route value, a query parameter or a body field is not explained
// (in backticks, like `from`) in the endpoint's description.
[Trait("Category", "Unit")]
public class EndpointDocumentationTests
{
    private const int MinimumDescriptionLength = 80;

    private static readonly HashSet<Type> _simple =
    [
        typeof(string), typeof(Guid), typeof(DateOnly), typeof(DateTime), typeof(TimeOnly),
        typeof(bool), typeof(int), typeof(long), typeof(decimal), typeof(double),
    ];

    private static bool IsSimple(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || _simple.Contains(type);
    }

    private static string Camel(string name) => char.ToLowerInvariant(name[0]) + name[1..];

    // What the framework binds from the query string for an [AsParameters] type: the constructor's
    // parameters when it has one, otherwise the settable properties.
    private static IEnumerable<string> AsParametersNames(Type type)
    {
        var constructor = type.GetConstructors().SingleOrDefault(c => c.GetParameters().Length > 0);
        return constructor is not null
            ? constructor.GetParameters().Select(p => p.Name!)
            : type.GetProperties().Where(p => p.SetMethod is { IsPublic: true }).Select(p => p.Name);
    }

    private static IEnumerable<(string Kind, string Name)> ParametersOf(RouteEndpoint endpoint)
    {
        var method = endpoint.Metadata.GetMetadata<MethodInfo>();
        if (method is null)
            yield break;

        var routeNames = endpoint
            .RoutePattern.Parameters.Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in method.GetParameters())
        {
            var type = parameter.ParameterType;
            var fromQuery = parameter.GetCustomAttribute<FromQueryAttribute>();

            if (parameter.GetCustomAttribute<AsParametersAttribute>() is not null)
            {
                foreach (var name in AsParametersNames(type))
                    yield return ("query", Camel(name));
            }
            else if (parameter.GetCustomAttribute<FromBodyAttribute>() is not null)
            {
                foreach (var property in type.GetProperties())
                    yield return ("body", Camel(property.Name));
            }
            else if (IsSimple(type))
            {
                var name = fromQuery?.Name ?? parameter.Name!;
                yield return (routeNames.Contains(name) ? "route" : "query", Camel(name));
            }
        }
    }

    private static List<RouteEndpoint> Endpoints()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();

        var group = app.MapGroup("/api/v1");
        group.MapApplicationEndpoints(new ApplicationInstrumentation());

        return [.. ((IEndpointRouteBuilder)app).DataSources.SelectMany(d => d.Endpoints).OfType<RouteEndpoint>()];
    }

    [Fact]
    public void EveryEndpoint_HasASummaryAndADescription()
    {
        var thin = Endpoints()
            .Where(e =>
                string.IsNullOrWhiteSpace(e.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary)
                || (e.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description?.Length ?? 0)
                    < MinimumDescriptionLength
            )
            .Select(e => e.DisplayName)
            .ToList();

        Assert.True(thin.Count == 0, "Missing a summary or description:\n" + string.Join("\n", thin));
    }

    [Fact]
    public void EveryParameter_IsExplainedInItsEndpointsDescription()
    {
        var gaps = new List<string>();

        foreach (var endpoint in Endpoints())
        {
            var description = endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description ?? "";
            var missing = ParametersOf(endpoint)
                .Where(p => !description.Contains($"`{p.Name}`", StringComparison.Ordinal))
                .Select(p => $"{p.Kind} `{p.Name}`")
                .Distinct()
                .ToList();

            if (missing.Count > 0)
            {
                var verbs = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
                gaps.Add($"{string.Join("/", verbs ?? [])} {endpoint.RoutePattern.RawText}: {string.Join(", ", missing)}");
            }
        }

        Assert.True(gaps.Count == 0, $"{gaps.Count} endpoints leave a parameter unexplained:\n" + string.Join("\n", gaps));
    }
}
