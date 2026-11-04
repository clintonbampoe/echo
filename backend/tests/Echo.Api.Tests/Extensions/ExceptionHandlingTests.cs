using System.Diagnostics;
using System.Text.Json;
using Echo.Api.Extensions;
using Echo.Shared.Services.Hashing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class ExceptionHandlingTests
{
    private static async Task<(int Status, JsonElement Body, string Raw)> WriteServerError(
        HttpContext context
    )
    {
        context.Response.Body = new MemoryStream();

        await ExceptionHandlingExtensions.WriteServerError(
            context,
            new InvalidOperationException("secret database detail"),
            isDevelopment: false
        );

        context.Response.Body.Position = 0;
        var raw = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, JsonDocument.Parse(raw).RootElement.Clone(), raw);
    }

    [Fact]
    public async Task ServerError_IncludesTheRequestTraceIdentifier()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "request-trace-1" };

        var (status, body, _) = await WriteServerError(context);

        Assert.Equal(500, status);
        Assert.Equal("request-trace-1", body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ServerError_PrefersTheCurrentActivityTraceId()
    {
        using var activity = new Activity("test").Start();
        var context = new DefaultHttpContext { TraceIdentifier = "request-trace-1" };

        var (_, body, _) = await WriteServerError(context);

        Assert.Equal(activity.TraceId.ToString(), body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ServerError_InProduction_DoesNotLeakTheExceptionMessage()
    {
        var (_, _, raw) = await WriteServerError(new DefaultHttpContext());

        Assert.DoesNotContain("secret database detail", raw);
    }

    [Fact]
    public async Task BadRequest_IncludesTheRequestTraceIdentifier()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "request-trace-2" };
        context.Response.Body = new MemoryStream();

        await ExceptionHandlingExtensions.WriteBadRequest(
            context,
            new BadHttpRequestException("bad"),
            isDevelopment: false
        );

        context.Response.Body.Position = 0;
        var body = JsonDocument.Parse(context.Response.Body).RootElement;
        Assert.Equal("request-trace-2", body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task WhenPasswordChecksAreBacklogged_AnyRouteAnswers503_WithRetryAfter()
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        context.Response.Body = new MemoryStream();

        await ExceptionHandlingExtensions.WriteBusy(
            context,
            new HashingBusyException(TimeSpan.FromSeconds(4))
        );

        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("4", context.Response.Headers.RetryAfter.ToString());
    }
}
