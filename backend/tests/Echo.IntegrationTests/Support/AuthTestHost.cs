using System.Security.Cryptography;
using System.Text;
using Echo.Application;
using Echo.Auth;
using Echo.Data;
using Echo.Domain.Congregations;
using Echo.Shared.Extensions;
using Echo.Shared.Options.Jwt;
using Echo.Shared.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Echo.IntegrationTests.Support;

/// <summary>
/// The auth services wired the way the API wires them, against the fixture's Postgres, in one
/// long-lived scope (like a single request). Tests swap services through <c>configure</c>.
/// </summary>
public sealed class AuthTestHost : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    private AuthTestHost(ServiceProvider provider, CommandCounter commands, FakeTimeProvider time)
    {
        _provider = provider;
        _scope = provider.CreateScope();
        Commands = commands;
        Time = time;
    }

    public CommandCounter Commands { get; }

    public FakeTimeProvider Time { get; }

    public T Get<T>()
        where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();

    /// <summary>Inserts a congregation (users and invitations need one) and returns its id.</summary>
    public async Task<Guid> SeedCongregationAsync()
    {
        var context = Get<AppDbContext>();
        var congregation = new Congregation
        {
            Id = Guid.NewGuid(),
            Name = $"Grace Chapel {Guid.NewGuid():N}",
            PhoneNumber = "0200000000",
            EmailAddress = "office@echo.test",
            City = "Accra",
            Town = "Osu",
            GpsAddress = "GA-000-0000",
        };
        context.Add(congregation);
        await context.SaveChangesAsync();
        return congregation.Id;
    }

    public IServiceScope NewScope() => _provider.CreateScope();

    /// <summary>Empties the database, then builds a host on it.</summary>
    public static async Task<AuthTestHost> CreateAsync(
        PostgresFixture db,
        Action<IServiceCollection>? configure = null
    )
    {
        await db.ResetAsync();

        var (privateKey, publicKey) = NewKeyPair();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "echo-test",
                    ["Jwt:Audience"] = "echo-test",
                    ["Jwt:PrivateKey"] = privateKey,
                    ["Jwt:PublicKey"] = publicKey,
                    ["FrontendClient:BaseUrl"] = "https://echo.test",
                    ["Frontend:BaseUrl"] = "https://echo.test",
                    ["Resend:ApiKey"] = "re_test",
                    ["Invitations:CodeKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                }
            )
            .Build();

        var commands = new CommandCounter();
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));

        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new TestEnvironment());
        services
            .AddApplicationServices()
            .AddAuthServices(configuration)
            .AddDataServices()
            .AddSharedServices(configuration);
        services.AddSingleton(new JwtKeyRing(configuration.GetSection("Jwt").Get<JwtOptions>()!));
        services.AddSingleton<TimeProvider>(time);
        services.AddDbContext<AppDbContext>(
            (provider, options) =>
            {
                options.UseNpgsql(db.ConnectionString).AddInterceptors(commands);
                options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
                options.ConfigureEchoDatabase();
            }
        );
        configure?.Invoke(services);

        return new AuthTestHost(services.BuildServiceProvider(), commands, time);
    }

    public async ValueTask DisposeAsync()
    {
        _scope.Dispose();
        await _provider.DisposeAsync();
    }

    private static (string Private, string Public) NewKeyPair()
    {
        using var rsa = RSA.Create(2048);
        return (
            Convert.ToBase64String(Encoding.UTF8.GetBytes(rsa.ExportPkcs8PrivateKeyPem())),
            Convert.ToBase64String(Encoding.UTF8.GetBytes(rsa.ExportSubjectPublicKeyInfoPem()))
        );
    }
}

internal sealed class TestEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "Echo.IntegrationTests";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
