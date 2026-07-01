using System.Text.Json.Serialization;
using Echo.Auth.Extensions;
using Echo.Core.Extensions;
using Echo.Domain.Data;
using Echo.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

EnvLoader.LoadFromRepoRoot();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// 1. GET THE TEMPLATE AND INJECT THE SECRETS
var template =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection template not found.");

var connectionString = template
    .Replace("__DB_USER__", builder.Configuration["DB_USER"])
    .Replace("__DB_PASSWORD__", builder.Configuration["DB_PASSWORD"]);

// REGISTER SERVICES
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, x => x.MigrationsAssembly("Echo.Infrastructure"))
);

builder.Services.AddCoreServices();
builder.Services.AddAuthServices(builder.Configuration);

builder
    .Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

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
