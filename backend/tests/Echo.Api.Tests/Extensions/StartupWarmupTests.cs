using Echo.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Echo.Api.Tests.Extensions;

[Trait("Category", "Unit")]
public class StartupWarmupTests
{
    private sealed class BrokenScopes : IServiceScopeFactory
    {
        public IServiceScope CreateScope() =>
            throw new InvalidOperationException("database is down");
    }

    [Fact]
    public async Task WhenTheDatabaseIsDown_TheWarmupGivesUp_AndTheApiStillStarts()
    {
        var warmup = new StartupWarmup(new BrokenScopes(), NullLogger<StartupWarmup>.Instance);

        await warmup.StartAsync(CancellationToken.None);
    }
}
