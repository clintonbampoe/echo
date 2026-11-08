using System.Security.Claims;
using System.Text.Encodings.Web;
using Echo.Api.Extensions;
using Echo.Application;
using Echo.Application.Caching;
using Echo.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Echo.Api.Tests.Extensions;

// Pieces that are right on their own and wrong together: a cache placed before sign-in caches nothing, and a
// context built without the interceptor never invalidates anything. Neither shows up as an error.
[Trait("Category", "Unit")]
public class ReadCacheWiringTests
{
    private sealed class SignedInByHeader(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-User", out var user))
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(
                [
                    new Claim("sub", user.ToString()),
                    new Claim("congregationId", Guid.NewGuid().ToString()),
                ],
                Scheme.Name
            );
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    [Fact]
    public async Task InTheRealPipeline_ASignedInReadIsCached()
    {
        var reads = 0;
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder
            .Services.AddApplicationServices()
            .AddReadCache(builder.Configuration)
            .ConfigureForwardedHeaders(builder.Configuration)
            .ConfigureRateLimits()
            .ConfigureCors(builder.Configuration);
        builder
            .Services.AddAuthentication("test")
            .AddScheme<AuthenticationSchemeOptions, SignedInByHeader>("test", null);
        builder.Services.AddAuthorization();
        await using var app = builder.Build();
        app.UseApiPipeline();
        app.MapGet("/things", () => Interlocked.Increment(ref reads))
            .RequireAuthorization()
            .CacheOutput(ReadCache.Policy);
        await app.StartAsync(TestContext.Current.CancellationToken);
        var client = app.GetTestClient();
        var ct = TestContext.Current.CancellationToken;

        for (var i = 0; i < 2; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/things");
            request.Headers.Add("X-User", "11111111-1111-1111-1111-111111111111");
            await client.SendAsync(request, ct);
        }

        Assert.Equal(1, reads);
    }

    [Fact]
    public async Task TheDatabaseContextTheApiUses_HasTheInterceptorAttached()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:echodb"] = "Host=localhost;Database=unused;Username=x;Password=x",
            }
        );
        builder.Services.AddApplicationServices().ConfigureDbContext(
            builder.Configuration,
            builder.Environment
        );
        await using var app = builder.Build();
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var interceptors = db.GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!
            .Interceptors;

        Assert.Contains(interceptors!, i => i is CacheInvalidationInterceptor);
    }
}
