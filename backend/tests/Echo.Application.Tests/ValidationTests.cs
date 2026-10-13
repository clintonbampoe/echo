using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Echo.Application.Members;
using Echo.Application.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Echo.Application.Tests;

// Real endpoints, real request pipeline, no database: every request below is rejected by
// validation before a service is called, so the services are never used (see Unused<T>).
[Trait("Category", "Unit")]
public class ValidationTests : IAsyncLifetime
{
    private readonly ApplicationInstrumentation _instrumentation = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddApplicationServices();
        builder
            .Services.AddAuthentication("test")
            .AddScheme<AuthenticationSchemeOptions, AdminAuthHandler>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddScoped(_ => Unused<MemberService>());
        builder.Services.AddScoped(_ => Unused<UserService>());

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapMemberEndpoints(_instrumentation);
        _app.MapUserEndpoints(_instrumentation);
        await _app.StartAsync();

        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        _instrumentation.Dispose();
    }

    // Signs every request in as an Admin so the role requirements on the routes are met.
    private sealed class AdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                [new Claim("sub", Guid.NewGuid().ToString()), new Claim("role", "Admin")],
                authenticationType: "test",
                nameType: "sub",
                roleType: "role"
            );
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    private static async Task<List<string>> RejectedFields(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonDocument
            .Parse(json)
            .RootElement.GetProperty("errors")
            .EnumerateObject()
            .Select(field => field.Name.ToLowerInvariant())
            .ToList();
    }

    private static T Unused<T>()
        where T : class => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    [Theory]
    [InlineData("/members/search?q=a")]
    [InlineData("/users/search?q=a")]
    public async Task Search_TermShorterThanTwoCharacters_Returns400(string url)
    {
        var res = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(["q"], await RejectedFields(res));
    }

    [Fact]
    public async Task Search_TermLongerThanOneHundredCharacters_Returns400()
    {
        var url = $"/members/search?q={new string('a', 101)}";

        var res = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(["q"], await RejectedFields(res));
    }

    [Fact]
    public async Task CreateUser_PasswordShorterThanEightCharacters_Returns400()
    {
        var body = new
        {
            firstName = "Ama",
            lastName = "Mensah",
            emailAddress = "ama@example.com",
            password = "short",
        };

        var res = await _client.PostAsJsonAsync(
            "/users",
            body,
            TestContext.Current.CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal(["password"], await RejectedFields(res));
    }
}
