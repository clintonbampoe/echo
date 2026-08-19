using System.Text.Json.Serialization;
using Echo.Api.Extensions;
using Echo.Application.Extensions;
using Echo.Auth.Extensions;
using Echo.Core.Extensions;
using Echo.Domain.Data;
using Echo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddOpenApi();
builder.Services.AddApiVersioningSetup();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRateLimitingToEndpoints();
builder.Services.AddHealthCheckServices();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

builder.Services.AddCoreServices();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.UseScalarDocumentation();
    app.MapOpenApi().AllowAnonymous();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
