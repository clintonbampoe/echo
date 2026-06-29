using System.Text.Json.Serialization;
using Api.Auth.Common.ExtensionMethods;
using Api.Core.Data;
using Api.Core.Repositories;
using Api.Core.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var template = GetConnectionStringTemplate();
var (username, password) = GetLocalDatabaseCredentials();
var connectionString = BuildConnectionStringFromTemplate(template);

// AUTH
builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, opt => opt.SetPostgresVersion(18, 0))
);

builder
    .Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

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
builder.Services.AddScoped<AttendanceTypeRepository>();
builder.Services.AddScoped<AttendanceContextRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<AssetCategoryService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<EventAttendanceService>();
builder.Services.AddScoped<EventRegistrationService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<OrganizationMemberService>();
builder.Services.AddScoped<OrganizationService>();
builder.Services.AddScoped<ProjectCategoryService>();
builder.Services.AddScoped<ProjectContributionService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TitheService>();
builder.Services.AddScoped<TransactionCategoryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<AttendanceTypeService>();
builder.Services.AddScoped<AttendanceContextService>();
builder.Services.AddScoped<UserService>();

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
