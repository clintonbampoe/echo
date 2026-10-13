using Asp.Versioning;
using Echo.Api.Extensions;
using Echo.Application;
using Echo.Auth;
using Echo.Data;
using Echo.ServiceDefaults;
using Echo.Shared.Extensions;
using Echo.Shared.HttpResults;
using Echo.Shared.Options.Api;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.ConfigureAuthObservability();
builder.AddServiceDefaults();

builder
    .Services.AddOpenApi()
    .AddApplicationServices()
    .AddAuthServices(builder.Configuration)
    .AddDataServices()
    .AddSharedServices();

builder
    .Services.ConfigureApiOptions()
    .ConfigureInvalidModelStateResponse()
    .ConfigureApiRouting()
    .ConfigureControllerOptions()
    .ConfigureJsonSerializer()
    .ConfigureSwaggerDocs()
    .ConfigureDbContext(builder.Configuration)
    .ConfigureApiVersioning()
    .ConfigureForwardedHeaders(builder.Configuration)
    .ConfigureRateLimits()
    .ConfigureHealthChecks()
    .ConfigureJwtAuthentication(builder.Configuration)
    .ConfigureCors(builder.Configuration);

var app = builder.Build();

// Problem responses link their "type" to the API's base URL; the options are validated at startup.
ErrorTypes.UseBaseUrl(app.Services.GetRequiredService<IOptions<ApiOptions>>().Value.BaseUrl);

// Must run before anything that reads the client IP or scheme (the rate limiter, auth).
app.UseForwardedHeaders();
app.UseSecurityHeaders();

await Database.RunMigrationsOnStartup(app, builder.Configuration);

var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build();
RouteGroupBuilder v1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1, 0);

var instrumentation = app.Services.GetRequiredService<ApplicationInstrumentation>();
var authInstrumentation = app.Services.GetRequiredService<AuthInstrumentation>();

v1.MapApplicationEndpoints(instrumentation);
v1.MapAuthEndpoints(authInstrumentation);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUi();
    app.UseScalarUi();
    app.MapOpenApi().AllowAnonymous();
}

app.ConfigureExceptionHandler();

app.UseRouting();
app.UseCors(Cors.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
