using Echo.Application.Caching;
using Echo.Application.Tests.Caching;
using Echo.Application.Attendances;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class AttendanceTotalsRefresherTests
{
    // Counts the refreshes, and fails each one, as a database that is down would.
    private sealed class FailingScopes : IServiceScopeFactory
    {
        public int Attempts;

        public IServiceScope CreateScope()
        {
            Attempts++;
            throw new InvalidOperationException("database is down");
        }
    }

    [Fact]
    public async Task ARefreshThatFails_IsTriedAgainAtTheNextInterval()
    {
        var time = new FakeTimeProvider();
        var scopes = new FailingScopes();
        using var worker = new AttendanceTotalsRefresher(
            scopes,
            new ReadCacheInvalidator(new RecordingCacheStore(), NullLogger<ReadCacheInvalidator>.Instance),
            time,
            NullLogger<AttendanceTotalsRefresher>.Instance
        );

        await worker.StartAsync(CancellationToken.None);
        for (var i = 0; i < 2; i++)
        {
            await Task.Delay(50, TestContext.Current.CancellationToken);
            time.Advance(AttendanceTotalsRefresher.Interval);
        }
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(2, scopes.Attempts);
    }

    [Fact]
    public async Task NothingIsRefreshed_BeforeTheFirstInterval()
    {
        var time = new FakeTimeProvider();
        var scopes = new FailingScopes();
        using var worker = new AttendanceTotalsRefresher(
            scopes,
            new ReadCacheInvalidator(new RecordingCacheStore(), NullLogger<ReadCacheInvalidator>.Instance),
            time,
            NullLogger<AttendanceTotalsRefresher>.Instance
        );

        await worker.StartAsync(CancellationToken.None);
        time.Advance(AttendanceTotalsRefresher.Interval - TimeSpan.FromSeconds(1));
        await Task.Delay(50, TestContext.Current.CancellationToken);
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(0, scopes.Attempts);
    }
}
