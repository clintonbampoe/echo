using System.Collections.Concurrent;
using System.Security.Claims;
using Echo.Application.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.Application.Tests.Caching;

[Trait("Category", "Unit")]
public class ReadCacheEvictionsTests
{
    private static readonly Guid Alpha = Guid.NewGuid();
    private static readonly Guid Beta = Guid.NewGuid();
    private static readonly Guid Person = Guid.NewGuid();

    // Keeps whatever it is given and never removes any of it, however it is asked.
    private sealed class ForgetfulStore : IOutputCacheStore
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

    private sealed class DownStore : IOutputCacheStore
    {
        public CancellationToken Seen;

        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
        {
            Seen = cancellationToken;
            throw new InvalidOperationException("the store is down");
        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken) =>
            ValueTask.FromResult<byte[]?>(null);

        public ValueTask SetAsync(
            string key,
            byte[] value,
            string[]? tags,
            TimeSpan validFor,
            CancellationToken cancellationToken
        ) => ValueTask.CompletedTask;
    }

    private static ReadCacheEvictions Evictions(IOutputCacheStore? store = null) =>
        new(store ?? new RecordingCacheStore(), NullLogger<ReadCacheEvictions>.Instance);

    [Fact]
    public async Task ADrop_ChangesTheVersionOfThatCongregationOnly()
    {
        var evictions = Evictions();
        var before = (evictions.Version(Alpha, false), evictions.Version(Beta, false));

        await evictions.Drop(Alpha);

        Assert.NotEqual(before.Item1, evictions.Version(Alpha, false));
        Assert.Equal(before.Item2, evictions.Version(Beta, false));
    }

    [Fact]
    public async Task ADropOfTheViews_ChangesTheVersionOfReadsFromViewsOnly()
    {
        var evictions = Evictions();
        var plain = evictions.Version(Alpha, readsViews: false);
        var fromViews = evictions.Version(Alpha, readsViews: true);

        await evictions.DropViews();

        Assert.Equal(plain, evictions.Version(Alpha, readsViews: false));
        Assert.NotEqual(fromViews, evictions.Version(Alpha, readsViews: true));
    }

    // A drop runs after a write has committed, so a store that fails must not turn that write into an error,
    // and the answers must be unreachable even though they could not be removed.
    [Fact]
    public async Task ADropThatFails_DoesNotThrow_AndStillMakesOldAnswersUnreachable()
    {
        var evictions = Evictions(new DownStore());
        var before = evictions.Version(Alpha, false);

        await evictions.Drop(Alpha);

        Assert.NotEqual(before, evictions.Version(Alpha, false));
    }

    [Fact]
    public async Task ADrop_IsNeverCancelled_ByTheRequestThatCausedIt()
    {
        var store = new DownStore();

        await Evictions(store).DropViews();

        Assert.False(store.Seen.CanBeCanceled);
    }

    [Fact]
    public void ASynchronousSave_FailsLoudly_RatherThanSkippingTheDrop()
    {
        var interceptor = new CacheEvictionInterceptor(Evictions());

        Assert.Throws<NotSupportedException>(() => interceptor.SavingChanges(null!, default));
    }

    // The point of the versions: being right must not depend on anything having been removed. This store
    // removes nothing, and a read that was already under way when the drop happened stores its answer after it.
    private sealed class Rig : IAsyncDisposable
    {
        public int Reads;
        public WebApplication App = null!;
        public HttpClient Client = null!;

        public static async Task<Rig> Start()
        {
            var rig = new Rig();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton<ApplicationInstrumentation>();
            builder.Services.AddSingleton<ReadCacheEvictions>();
            builder.Services.AddReadCache(builder.Configuration);
            builder.Services.AddSingleton<IOutputCacheStore, ForgetfulStore>();
            rig.App = builder.Build();
            rig.App.Use(
                (context, next) =>
                {
                    var claims = new Claim[]
                    {
                        new("sub", Person.ToString()),
                        new("congregationId", Alpha.ToString()),
                    };
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
                    return next();
                }
            );
            rig.App.UseOutputCache();
            rig.App.MapGet("/read", () => Interlocked.Increment(ref rig.Reads))
                .CacheOutput(ReadCache.Policy);
            rig.App.MapGet(
                    "/read-while-written",
                    async (ReadCacheEvictions evictions) =>
                    {
                        var read = Interlocked.Increment(ref rig.Reads);
                        await evictions.Drop(Alpha);
                        return read;
                    }
                )
                .CacheOutput(ReadCache.Policy);
            await rig.App.StartAsync();
            rig.Client = rig.App.GetTestClient();
            return rig;
        }

        public ValueTask DisposeAsync() => App.DisposeAsync();
    }

    private static async Task Ask(Rig rig, string url) =>
        await rig.Client.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, url),
            TestContext.Current.CancellationToken
        );

    [Fact]
    public async Task AfterADrop_NoOldAnswerIsServed_EvenWhenNothingWasRemoved()
    {
        await using var rig = await Rig.Start();
        var evictions = rig.App.Services.GetRequiredService<ReadCacheEvictions>();

        await Ask(rig, "/read");
        await Ask(rig, "/read");
        Assert.Equal(1, rig.Reads);

        await evictions.Drop(Alpha);
        await Ask(rig, "/read");

        Assert.Equal(2, rig.Reads);
    }

    [Fact]
    public async Task ARead_ThatStoresItsAnswerAfterTheDrop_IsNeverServedToTheNextRead()
    {
        await using var rig = await Rig.Start();

        await Ask(rig, "/read-while-written");
        await Ask(rig, "/read-while-written");

        Assert.Equal(2, rig.Reads);
    }
}
