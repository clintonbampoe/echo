using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Echo.Api.Extensions;
using Echo.Application;
using Echo.Auth;
using Echo.Auth.Registrations;
using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Api.Tests.Extensions;

// The real Auth endpoint and the real validation setup, no database: every request below is
// rejected before RegistrationService is called, so it is never used.
[Trait("Category", "Unit")]
public class ValidationProblemTests : IAsyncLifetime
{
    private readonly AuthInstrumentation _instrumentation = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddApplicationServices();
        builder.Services.AddAuthServices(builder.Configuration);
        builder.Services.ConfigureValidationProblemDetails();
        builder.Services.AddScoped(_ =>
            (RegistrationService)RuntimeHelpers.GetUninitializedObject(typeof(RegistrationService))
        );

        _app = builder.Build();
        _app.UseRouting();
        _app.MapGroup("/auth").MapRegistrationEndpoints(_instrumentation);
        await _app.StartAsync();

        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        _instrumentation.Dispose();
    }

    private async Task<(HttpStatusCode Status, JsonElement Body)> RegisterMember(object body)
    {
        var res = await _client.PostAsJsonAsync(
            "/auth/registrations/member",
            body,
            TestContext.Current.CancellationToken
        );
        var json = await res.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return (res.StatusCode, JsonDocument.Parse(json).RootElement);
    }

    private static object ValidUser(string password = "long-enough-1") =>
        new
        {
            firstName = "Ama",
            lastName = "Mensah",
            emailAddress = "ama@example.com",
            password,
        };

    [Fact]
    public async Task AuthDto_EmptyToken_IsRejected()
    {
        var (status, body) = await RegisterMember(new { token = "", userInfo = ValidUser() });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.True(body.GetProperty("errors").TryGetProperty("Token", out _));
    }

    [Fact]
    public async Task AuthDto_NestedShortPassword_IsRejected()
    {
        var (status, body) = await RegisterMember(
            new { token = "abc", userInfo = ValidUser("short") }
        );

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains(
            body.GetProperty("errors").EnumerateObject(),
            field => field.Name.Contains("Password", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public async Task ValidationError_CarriesTheSameFieldsAsEveryOtherProblem()
    {
        var (status, body) = await RegisterMember(new { token = "", userInfo = ValidUser() });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.Equal(ErrorTypes.ValidationError, body.GetProperty("type").GetString());
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
        Assert.True(body.GetProperty("errors").EnumerateObject().Any());
    }
}
