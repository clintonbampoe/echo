using Microsoft.AspNetCore.OutputCaching;

namespace Echo.Application.Tests.Caching;

// Stands for the output cache's store and remembers which tags were dropped.
public sealed class RecordingCacheStore : IOutputCacheStore
{
    public List<string> Evicted { get; } = [];

    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        Evicted.Add(tag);
        return ValueTask.CompletedTask;
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
