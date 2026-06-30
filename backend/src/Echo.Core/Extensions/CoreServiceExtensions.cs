using Echo.Core.Repositories;
using Echo.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<MemberRepository>();
        services.AddScoped<AssetRepository>();
        services.AddScoped<AssetCategoryRepository>();
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<EventAttendanceRepository>();
        services.AddScoped<EventRegistrationRepository>();
        services.AddScoped<EventRepository>();
        services.AddScoped<OrganizationMemberRepository>();
        services.AddScoped<OrganizationRepository>();
        services.AddScoped<ProjectCategoryRepository>();
        services.AddScoped<ProjectContributionRepository>();
        services.AddScoped<ProjectRepository>();
        services.AddScoped<TitheRepository>();
        services.AddScoped<TransactionCategoryRepository>();
        services.AddScoped<TransactionRepository>();
        services.AddScoped<AttendanceTypeRepository>();
        services.AddScoped<AttendanceContextRepository>();
        services.AddScoped<UserRepository>();

        services.AddScoped<MemberService>();
        services.AddScoped<AssetService>();
        services.AddScoped<AssetCategoryService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<EventAttendanceService>();
        services.AddScoped<EventRegistrationService>();
        services.AddScoped<EventService>();
        services.AddScoped<OrganizationMemberService>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<ProjectCategoryService>();
        services.AddScoped<ProjectContributionService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<TitheService>();
        services.AddScoped<TransactionCategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<AttendanceTypeService>();
        services.AddScoped<AttendanceContextService>();
        services.AddScoped<UserService>();

        // first parameter is ignored because we have no configurations outside our AutoMapper profiles
        services.AddAutoMapper(_ => { }, typeof(CoreServiceExtensions));

        return services;
    }
}
