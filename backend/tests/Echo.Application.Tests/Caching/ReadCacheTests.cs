using System.Security.Claims;
using Echo.Application.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Tests.Caching;

// Runs the real policy on a real pipeline: the answer to "who may be given a cached answer" is the
// part of the cache that must never be wrong.
[Trait("Category", "Unit")]
public class ReadCacheTests : IAsyncLifetime
{
    private static readonly Guid Alpha = Guid.NewGuid();
    private static readonly Guid Beta = Guid.NewGuid();
    private static readonly Guid Ann = Guid.NewGuid();
    private static readonly Guid Ben = Guid.NewGuid();

    private int _reads;
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddReadCache(builder.Configuration);
        builder.Services.AddSingleton<ApplicationInstrumentation>();
        builder.Services.AddSingleton<ReadCacheEvictions>();
        _app = builder.Build();

        // Stands for the JWT: who is asking comes in two headers.
        _app.Use(
            (context, next) =>
            {
                if (context.Request.Headers.TryGetValue("X-User", out var user))
                {
                    var claims = new Claim[]
                    {
                        new("sub", user.ToString()),
                        new("congregationId", context.Request.Headers["X-Congregation"].ToString()),
                    };
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
                }
                return next();
            }
        );
        _app.UseOutputCache();

        var group = _app.MapGroup("/things").CacheOutput(ReadCache.Policy);
        group.MapGet("/", () => Interlocked.Increment(ref _reads));
        group.MapGet("/search", () => Interlocked.Increment(ref _reads))
            .CacheOutput(p => p.NoCache());
        group.MapGet(
            "/missing",
            () =>
            {
                Interlocked.Increment(ref _reads);
                return Results.NotFound();
            }
        );
        group.MapPost("/", () => Interlocked.Increment(ref _reads));
        _app.MapGet("/totals", () => Interlocked.Increment(ref _reads))
            .CacheOutput(ReadCache.ViewPolicy);
        // A read that is being worked out when a write drops the cache.
        group.MapGet(
            "/overlapped",
            async (ReadCacheEvictions cache) =>
            {
                var read = Interlocked.Increment(ref _reads);
                await cache.Drop(Alpha);
                return read;
            }
        );

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();

    private async Task<string> Send(
        HttpMethod method,
        string url,
        Guid? user,
        Guid? congregation,
        string? origin = null
    )
    {
        var request = new HttpRequestMessage(method, url);
        if (origin is not null)
            request.Headers.Add("Origin", origin);
        if (user is { } u)
            request.Headers.Add("X-User", u.ToString());
        if (congregation is { } c)
            request.Headers.Add("X-Congregation", c.ToString());
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
        return await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }

    private Task<string> Get(
        string url,
        Guid? user,
        Guid? congregation,
        string? origin = null
    ) => Send(HttpMethod.Get, url, user, congregation, origin);

    [Fact]
    public async Task TheSamePersonAskingTwice_IsAnsweredFromTheCache()
    {
        var first = await Get("/things", Ann, Alpha);
        var second = await Get("/things", Ann, Alpha);

        Assert.Equal(first, second);
        Assert.Equal(1, _reads);
    }

    [Fact]
    public async Task AnotherQuery_IsNotGivenTheAnswerToThisOne()
    {
        await Get("/things?page=1", Ann, Alpha);
        await Get("/things?page=2", Ann, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task AnotherPerson_NeverSeesAnswersKeptForThisOne_EvenInTheSameCongregation()
    {
        await Get("/things", Ann, Alpha);
        await Get("/things", Ben, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task ARequestNotSignedIn_IsNeverCached()
    {
        await Get("/things", user: null, congregation: null);
        await Get("/things", user: null, congregation: null);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task ASearch_IsNeverCached_EvenInAGroupThatIs()
    {
        await Get("/things/search", Ann, Alpha);
        await Get("/things/search", Ann, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task ARead_ThatOverlappedADrop_IsNotKept_ForItMayHoldWhatWasThereBefore()
    {
        await Get("/things/overlapped", Ann, Alpha);
        await Get("/things/overlapped", Ann, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task EachOrigin_GetsItsOwnAnswer_BecauseTheCorsHeadersAreKeptWithIt()
    {
        await Get("/things", Ann, Alpha, origin: "https://app.example");
        await Get("/things", Ann, Alpha, origin: "https://other.example");
        await Get("/things", Ann, Alpha, origin: "https://app.example");

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task OnlyAnOkAnswer_IsKept()
    {
        await Get("/things/missing", Ann, Alpha);
        await Get("/things/missing", Ann, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task APost_IsNeverCached()
    {
        await Send(HttpMethod.Post, "/things", Ann, Alpha);
        await Send(HttpMethod.Post, "/things", Ann, Alpha);

        Assert.Equal(2, _reads);
    }

    [Fact]
    public async Task DroppingACongregation_MakesItsNextReadFresh_AndLeavesOthersAlone()
    {
        var store = _app.Services.GetRequiredService<IOutputCacheStore>();
        await Get("/things", Ann, Alpha);
        await Get("/things", Ben, Beta);
        Assert.Equal(2, _reads);

        await store.EvictByTagAsync(ReadCache.Tag(Alpha), TestContext.Current.CancellationToken);

        await Get("/things", Ann, Alpha);
        await Get("/things", Ben, Beta);
        Assert.Equal(3, _reads);
    }

    [Fact]
    public async Task DroppingTheViews_MakesOnlyThoseReadsFresh()
    {
        var store = _app.Services.GetRequiredService<IOutputCacheStore>();
        await Get("/things", Ann, Alpha);
        await Get("/totals", Ann, Alpha);
        await Get("/totals", Ben, Beta);
        Assert.Equal(3, _reads);

        await store.EvictByTagAsync(ReadCache.Views, TestContext.Current.CancellationToken);

        await Get("/things", Ann, Alpha);
        await Get("/totals", Ann, Alpha);
        await Get("/totals", Ben, Beta);
        Assert.Equal(5, _reads);
    }
}
