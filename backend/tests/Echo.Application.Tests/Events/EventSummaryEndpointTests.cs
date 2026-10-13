using Echo.Application.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Tests.Events;

[Trait("Category", "Unit")]
public class EventSummaryEndpointTests
{
    [Fact]
    public void EventSummary_IsMappedAsAGetOnSlashSummary()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();
        using var instrumentation = new ApplicationInstrumentation();
        app.MapEventEndpoints(instrumentation);

        var summary = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SingleOrDefault(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "GetEventSummary");

        Assert.NotNull(summary);
        Assert.Equal("/events/summary", summary.RoutePattern.RawText);
        Assert.Contains("GET", summary.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
    }
}
