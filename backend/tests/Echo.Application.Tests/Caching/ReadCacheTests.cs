using System.Collections.Concurrent;
using System.Security.Claims;
using Echo.Application.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Tests.Caching;

// Runs the real policy on a real pipeline: who may be given a cached answer, and when an answer stops being given,
// is the part of the cache that must never be wrong. The store here never removes anything, so every test also
// shows that a stale answer is kept out by the version in its key and not by being removed.
[Trait("Category", "Unit")]
public class ReadCacheTests : IAsyncLifetime
{
    private static readonly Guid Alpha = Guid.NewGuid();
    private static readonly Guid Beta = Guid.NewGuid();
    private static readonly Guid Ann = Guid.NewGuid();
    private static readonly Guid Ben = Guid.NewGuid();

    private sealed class KeepsEverythingStore : IOutputCacheStore
    {
        private readonly ConcurrentDictionary<string, byte[]> _kept = new();

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken) =>
            ValueTask.CompletedTask;

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) =>
            ValueTask.FromResult(_kept.GetValueOrDefault(key));

        public ValueTask SetAsync(
            string key,
            byte[] value,
            string[]? tags,
            TimeSpan validFor,
            CancellationToken cancellationToken
        )
        {
            _kept[key] = value;
            return ValueTask.CompletedTask;
        }
    }

    private int _reads;
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private ReadCacheInvalidator _invalidator = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ApplicationInstrumentation>();
        builder.Services.AddSingleton<ReadCacheInvalidator>();
        builder.Services.AddReadCache(builder.Configuration);
        builder.Services.AddSingleton<IOutputCacheStore, KeepsEverythingStore>();
        _app = builder.Build();
        _invalidator = _app.Services.GetRequiredService<ReadCacheInvalidator>();

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
        group.MapGet("/", Read);
        group.MapGet("/search", Read).CacheOutput(p => p.NoCache());
        group.MapGet("/missing", () => Results.NotFound()).AddEndpointFilter(CountRead);
        group.MapPost("/", Read);
        group.MapGet(
            "/while-written",
            async (ReadCacheInvalidator invalidator) =>
            {
                var read = Read();
                await invalidator.Invalidate(Alpha);
                return read;
            }
        );
        _app.MapGet("/totals", Read).CacheOutput(ReadCache.ViewPolicy);

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();

    private int Read() => Interlocked.Increment(ref _reads);

    private ValueTask<object?> CountRead(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        Read();
        return next(context);
    }

    private async Task Send(
        HttpMethod method,
        string url,
        Guid? user,
        Guid? congregation,
        string? origin = null
    )
    {
        var request = new HttpRequestMessage(method, url);
        if (user is { } u)
            request.Headers.Add("X-User", u.ToString());
        if (congregation is { } c)
            request.Headers.Add("X-Congregation", c.ToString());
        if (origin is not null)
            request.Headers.Add("Origin", origin);
        await _client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private Task Get(string url, Guid? user, Guid? congregation, string? origin = null) =>
        Send(HttpMethod.Get, url, user, congregation, origin);

    [Fact]
    public async Task TheSamePersonAskingTwice_IsAnsweredFromTheCache()
    {
        await Get("/things", Ann, Alpha);
        await Get("/things", Ann, Alpha);

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
    public async Task EachOrigin_GetsItsOwnAnswer_BecauseTheCorsHeadersAreKeptWithIt()
    {
        await Get("/things", Ann, Alpha, origin: "https://app.example");
        await Get("/things", Ann, Alpha, origin: "https://other.example");
        await Get("/things", Ann, Alpha, origin: "https://app.example");

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
    public async Task InvalidatingACongregation_MakesItsNextReadFresh_AndLeavesOthersAlone()
    {
        await Get("/things", Ann, Alpha);
        await Get("/things", Ben, Beta);

        await _invalidator.Invalidate(Alpha);

        await Get("/things", Ann, Alpha);
        await Get("/things", Ben, Beta);
        Assert.Equal(3, _reads);
    }

    [Fact]
    public async Task InvalidatingTheViews_MakesOnlyTheReadsFromViewsFresh()
    {
        await Get("/things", Ann, Alpha);
        await Get("/totals", Ann, Alpha);

        await _invalidator.InvalidateViews();

        await Get("/things", Ann, Alpha);
        await Get("/totals", Ann, Alpha);
        Assert.Equal(3, _reads);
    }

    [Fact]
    public async Task ARead_ThatStoresItsAnswerAfterTheWriteInvalidated_IsNeverGivenToTheNextRead()
    {
        await Get("/things/while-written", Ann, Alpha);
        await Get("/things/while-written", Ann, Alpha);

        Assert.Equal(2, _reads);
    }
}
