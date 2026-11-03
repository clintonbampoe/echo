using System.Diagnostics.Metrics;
using Echo.Application;
using Echo.Application.Caching;
using Echo.Shared.Caching;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Caching;

[Trait("Category", "Unit")]
public class SummaryCacheTests
{
    private sealed record Filters(string? Name, int? Year);

    private sealed record Payload(int Total);

    private sealed class CapturingLogger : ILogger<SummaryCache>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => Messages.Add(formatter(state, exception));
    }

    // How many times the summary was worked out, shared by every Owner the container hands out.
    private sealed class Calls
    {
        public int Computed;
        public bool Fail;
    }

    // Stands for a service such as TitheService: new for every request, so a refresh that runs after the
    // request has ended must ask the container for its own.
    private sealed class Owner(Calls calls)
    {
        public Task<Payload> Compute(CancellationToken _)
        {
            var n = Interlocked.Increment(ref calls.Computed);
            return calls.Fail && n > 1
                ? Task.FromException<Payload>(new InvalidOperationException("db is down"))
                : Task.FromResult(new Payload(n));
        }
    }

    private readonly CapturingLogger _logger = new();
    private readonly Calls _calls = new();
    private readonly FakeTimeProvider _time = new();
    private readonly ServiceProvider _services;
    private readonly SummaryCache _summaries;

    private int _computed => _calls.Computed;

    public SummaryCacheTests()
    {
        _services = new ServiceCollection()
            .AddSingleton(_calls)
            .AddScoped<Owner>()
            .BuildServiceProvider();
        var cache = new InMemoryCacheService(
            new MemoryCache(new MemoryCacheOptions()),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryCacheService>.Instance,
            Options.Create(new CacheOptions())
        );
        _summaries = new SummaryCache(
            cache,
            new ApplicationInstrumentation(),
            _services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new CacheOptions()),
            _time,
            _logger
        );
    }

    private async Task<IOperationResult> Serve(Guid congregationId, Filters filters)
    {
        // A request of its own: its own scope, ended when the request is.
        using var scope = _services.CreateScope();
        return await _summaries.Serve(
            scope.ServiceProvider.GetRequiredService<Owner>(),
            "things",
            congregationId,
            filters,
            (owner, token) => owner.Compute(token),
            CancellationToken.None
        );
    }

    private static async Task Until(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            Assert.True(
                DateTime.UtcNow < deadline,
                "Timed out waiting for the background refresh."
            );
            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task FirstRequest_ComputesTheSummary_AndSaysItWasNotCached()
    {
        var congregation = Guid.NewGuid();

        var result = await Serve(congregation, new Filters(null, 2026));

        Assert.Equal(new Payload(1), Assert.IsType<Ok<Payload>>(result.ToResult()).Value);
        Assert.Equal(1, _computed);
        Assert.Equal(
            [
                $"Summary things computed for congregation {congregation}",
                $"Summary things served for congregation {congregation} (cached: False)",
            ],
            _logger.Messages
        );
    }

    [Fact]
    public async Task SameRequestAgain_IsServedFromTheCache_WithoutComputing()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));
        _logger.Messages.Clear();

        var result = await Serve(congregation, new Filters(null, 2026));

        Assert.Equal(new Payload(1), Assert.IsType<Ok<Payload>>(result.ToResult()).Value);
        Assert.Equal(1, _computed);
        Assert.Equal(
            [$"Summary things served for congregation {congregation} (cached: True)"],
            _logger.Messages
        );
    }

    [Fact]
    public async Task EachRequest_IsCountedAsComputedOrCached_ForTheCacheHitRatio()
    {
        var seen = new List<(string Summary, bool Cached)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Name == "echo.summary.requests")
                l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>(
            (_, _, tags, _) =>
            {
                string? summary = null;
                var cached = false;
                foreach (var tag in tags)
                {
                    if (tag.Key == "summary")
                        summary = tag.Value as string;
                    if (tag.Key == "cached")
                        cached = tag.Value is true;
                }
                seen.Add((summary!, cached));
            }
        );
        listener.Start();
        var congregation = Guid.NewGuid();

        await Serve(congregation, new Filters(null, 2026));
        await Serve(congregation, new Filters(null, 2026));

        Assert.Equal([("things", false), ("things", true)], seen);
    }

    [Fact]
    public async Task ADifferentRequest_IsComputedAgain()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));

        await Serve(congregation, new Filters("Ama", 2026));

        Assert.Equal(2, _computed);
    }

    [Fact]
    public async Task ADifferentCongregation_NeverSeesAnothersSummary()
    {
        var filters = new Filters(null, 2026);
        await Serve(Guid.NewGuid(), filters);

        await Serve(Guid.NewGuid(), filters);

        Assert.Equal(2, _computed);
    }

    [Fact]
    public async Task LateInTheLifeOfAnEntry_TheRequestIsStillServedFromTheCache_WhileItIsRefreshedInTheBackground()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));
        _time.Advance(TimeSpan.FromSeconds(240)); // past three quarters of the five minutes

        var served = await Serve(congregation, new Filters(null, 2026));

        // The person who asked did not wait: they got the old answer...
        Assert.Equal(new Payload(1), Assert.IsType<Ok<Payload>>(served.ToResult()).Value);
        // ...and the next one gets the new one, without anyone having waited for it.
        await Until(() => _computed == 2);
        var next = await Serve(congregation, new Filters(null, 2026));
        Assert.Equal(new Payload(2), Assert.IsType<Ok<Payload>>(next.ToResult()).Value);
    }

    [Fact]
    public async Task EarlyInTheLifeOfAnEntry_NothingIsRefreshed()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));
        _time.Advance(TimeSpan.FromSeconds(120));

        await Serve(congregation, new Filters(null, 2026));
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(1, _computed);
    }

    [Fact]
    public async Task ManyRequestsInTheRefreshWindow_StartOneRefresh()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));
        _time.Advance(TimeSpan.FromSeconds(240));

        await Task.WhenAll(
            Enumerable.Range(0, 20).Select(_ => Serve(congregation, new Filters(null, 2026)))
        );
        await Until(() => _computed >= 2);
        await Task.Delay(100, TestContext.Current.CancellationToken);

        Assert.Equal(2, _computed);
    }

    [Fact]
    public async Task ARefreshThatFails_LeavesTheOldAnswerInPlace_AndIsTriedAgainByTheNextRequest()
    {
        var congregation = Guid.NewGuid();
        await Serve(congregation, new Filters(null, 2026));
        _calls.Fail = true;
        _time.Advance(TimeSpan.FromSeconds(240));

        await Serve(congregation, new Filters(null, 2026));
        await Until(() => _computed == 2);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        var again = await Serve(congregation, new Filters(null, 2026));

        Assert.Equal(new Payload(1), Assert.IsType<Ok<Payload>>(again.ToResult()).Value);
        await Until(() => _computed == 3);
    }
}
