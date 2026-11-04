using System.Text.Json;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Shared.Tests.HttpResults;

[Trait("Category", "Unit")]
public class ServiceBusyResultTests
{
    private static async Task<(DefaultHttpContext Context, JsonElement Body)> Execute(TimeSpan retryAfter)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        context.Response.Body = new MemoryStream();

        await new ServiceBusyResult(retryAfter).ToResult().ExecuteAsync(context);

        context.Response.Body.Position = 0;
        var raw = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context, JsonDocument.Parse(raw).RootElement.Clone());
    }

    [Fact]
    public async Task Answers503_WithRetryAfterInWholeSeconds()
    {
        var (context, _) = await Execute(TimeSpan.FromSeconds(3));

        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("3", context.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task RoundsAPartOfASecondUp_SoTheCallerNeverRetriesTooSoon()
    {
        var (context, _) = await Execute(TimeSpan.FromSeconds(2.2));

        Assert.Equal("3", context.Response.Headers.RetryAfter.ToString());
    }

    [Fact]
    public async Task TheProblemCarriesAStableCode_AndTheWaitInTheDetail()
    {
        var (_, body) = await Execute(TimeSpan.FromSeconds(7));

        Assert.Equal("SERVICE_BUSY", body.GetProperty("errorCode").GetString());
        Assert.Equal(7, body.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Contains("7 seconds", body.GetProperty("detail").GetString());
    }
}
