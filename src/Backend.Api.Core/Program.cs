using Backend.Api.Core.Data;
using Backend.Api.Core.Repositories.Engines;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var template = GetConnectionStringTemplate();
var (username, password) = GetLocalDatabaseCredentials();
var connectionString = BuildConnectionStringFromTemplate(template);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped(typeof(ICreateEntityEngine<>), typeof(CreateNewRecordEngine<>));
builder.Services.AddScoped(typeof(IGetEntityEngine<>), typeof(GetRecordEngine<>));
builder.Services.AddScoped(typeof(IUpdateEntityEngine<>), typeof(UpdateRecordEngine<>));
builder.Services.AddScoped(typeof(ISoftDeleteEntityEngine<>), typeof(SoftDeleteRecordEngine<>));
builder.Services.AddScoped(typeof(IDatabaseEngine<>), typeof(RecordEngine<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string? GetConnectionStringTemplate() =>
    builder.Configuration.GetConnectionString("DefaultConnection");

(string? username, string? password) GetLocalDatabaseCredentials()
{
    var dbUsername = builder.Configuration["Database:Username"];
    var dbPassword = builder.Configuration["Database:Password"];
    return (dbUsername, dbPassword);
}

string BuildConnectionStringFromTemplate(string? stringTemplate)
{
    return string.Format(stringTemplate!, username, password);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
