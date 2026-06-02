using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Services;
using Backend.Api.Core.Services.Interfaces;
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

builder.Services.AddScoped(typeof(ICreateNewRecordService<>), typeof(CreateNewRecordService<>));
builder.Services.AddScoped(typeof(IGetRecordService<>), typeof(GetRecordService<>));
builder.Services.AddScoped(typeof(IUpdateRecordService<>), typeof(UpdateRecordService<>));
builder.Services.AddScoped(typeof(ISoftDeleteService<>), typeof(SoftDeleteService<>));
builder.Services.AddScoped(typeof(IDomainRecordService<>), typeof(DomainRecordService<>));

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
