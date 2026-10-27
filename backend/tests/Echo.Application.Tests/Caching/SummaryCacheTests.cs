using Echo.Application;
using Echo.Application.Caching;
using Echo.Shared.Caching;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    private readonly CapturingLogger _logger = new();
    private readonly SummaryCache _summaries;
    private int _computed;

    public SummaryCacheTests()
    {
        var cache = new InMemoryCacheService(
            new MemoryCache(new MemoryCacheOptions()),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryCacheService>.Instance,
            Options.Create(new CacheOptions())
        );
        _summaries = new SummaryCache(cache, new ApplicationInstrumentation(), _logger);
    }

    private Task<IOperationResult> Serve(Guid congregationId, Filters filters) =>
        _summaries.Serve(
            "things",
            congregationId,
            filters,
            _ =>
            {
                _computed++;
                return Task.FromResult(new Payload(42));
            },
            CancellationToken.None
        );

    [Fact]
    public async Task FirstRequest_ComputesTheSummary_AndSaysItWasNotCached()
    {
        var congregation = Guid.NewGuid();

        var result = await Serve(congregation, new Filters(null, 2026));

        Assert.Equal(new Payload(42), Assert.IsType<Ok<Payload>>(result.ToResult()).Value);
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

        Assert.Equal(new Payload(42), Assert.IsType<Ok<Payload>>(result.ToResult()).Value);
        Assert.Equal(1, _computed);
        Assert.Equal(
            [$"Summary things served for congregation {congregation} (cached: True)"],
            _logger.Messages
        );
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
}
