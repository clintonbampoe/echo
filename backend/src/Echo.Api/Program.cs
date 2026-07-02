using System.Text.Json.Serialization;
using Echo.Api.Extensions;
using Echo.Auth.Extensions;
using Echo.Core.Extensions;
using Echo.Shared.Extensions;

EnvLoader.LoadFromRepoRoot();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContextServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddApiVersioningSetup();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
