using System.Text.Json.Serialization;
using Echo.Api.Extensions;
using Echo.Application.Extensions;
using Echo.Auth.Extensions;
using Echo.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddOpenApi();
builder.Services.AddApiVersioningSetup();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCoreServices();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddApplicationServices();
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
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
