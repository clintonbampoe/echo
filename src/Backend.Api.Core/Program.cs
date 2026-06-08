using Backend.Api.Core.Controllers;
using Backend.Api.Core.Data;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Repositories.Engines;
using Backend.Api.Core.Repositories.Engines.Interfaces;
using Backend.Api.Core.Repository;
using Backend.Api.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var template = GetConnectionStringTemplate();
var (username, password) = GetLocalDatabaseCredentials();
var connectionString = BuildConnectionStringFromTemplate(template);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddScoped(typeof(ICreateEntityEngine<>), typeof(CreateNewRecordEngine<>));
builder.Services.AddScoped(typeof(IGetEntityEngine<>), typeof(GetRecordEngine<>));
builder.Services.AddScoped(typeof(IUpdateEntityEngine<>), typeof(UpdateRecordEngine<>));
builder.Services.AddScoped(typeof(ISoftDeleteEntityEngine<>), typeof(SoftDeleteRecordEngine<>));
builder.Services.AddScoped(typeof(IDatabaseEngine<>), typeof(DatabaseEngine<>));

builder.Services.AddScoped<MemberRepository>();
builder.Services.AddScoped<AssetRepository>();
builder.Services.AddScoped<AssetCategoryRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<EventAttendanceRepository>();
builder.Services.AddScoped<EventRegistrationRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<OrganizationMemberRepository>();
builder.Services.AddScoped<OrganizationRepository>();
builder.Services.AddScoped<ProjectCategoryRepository>();
builder.Services.AddScoped<ProjectContributionRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<TitheRepository>();
builder.Services.AddScoped<TransactionCategoryRepository>();
builder.Services.AddScoped<TransactionRepository>();

builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<AssetCategoryService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<EventAttendanceService>();
builder.Services.AddScoped<EventRegistrationService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<OrganizationMemberRepository>();
builder.Services.AddScoped<OrganizationService>();
builder.Services.AddScoped<ProjectCategoryService>();
builder.Services.AddScoped<ProjectContributionService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TitheService>();
builder.Services.AddScoped<TransactionCategoryService>();
builder.Services.AddScoped<TransactionRepository>();

builder.Services.AddScoped<MembersController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
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
