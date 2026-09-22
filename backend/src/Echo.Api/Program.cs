using Echo.Api.Extensions;
using Echo.Application;
using Echo.Auth;
using Echo.Data;
using Echo.ServiceDefaults;
using Echo.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddDataServices();
builder.Services.AddSharedServices();

builder.Services.ConfigureApiRouting();
builder.Services.ConfigureControllerOptions();
builder.Services.ConfigureSwaggerDocs();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureRateLimits();
builder.Services.ConfigureHealthChecks();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureCors(builder.Configuration);

var app = builder.Build();
await Database.RunMigrationsOnStartup(app, builder.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUi();
    app.UseScalarUi();
    app.MapOpenApi().AllowAnonymous();
}

app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
