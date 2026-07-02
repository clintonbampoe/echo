using System.Text.Json.Serialization;
using Echo.Api.Extensions;
using Echo.Auth.Extensions;
using Echo.Core.Extensions;
using Echo.Shared.Extensions;

EnvLoader.LoadFromRepoRoot();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContextServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddOpenApi();
builder.Services.AddApiVersioningSetup();

builder.Services.AddCoreServices();
builder.Services.AddAuthServices(builder.Configuration);
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.UseScalarDocumentation();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
