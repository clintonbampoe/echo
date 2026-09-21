
using Echo.Application.Assets;
using Echo.Application.Attendances;
using Echo.Application.Congregations;
using Echo.Application.Events;
using Echo.Application.Members;
using Echo.Application.Organizations;
using Echo.Application.Projects;
using Echo.Application.Tithes;
using Echo.Application.Transactions;
using Echo.Application.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application;

public static class Extensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Members
        services.AddScoped<MemberRepository>();
        services.AddScoped<MemberService>();
        services.AddSingleton<IMemberMapper, MemberMapper>();

        // Users
        services.AddScoped<UserRepository>();
        services.AddSingleton<IUserMapper, UserMapper>();

        // Congregations
        services.AddScoped<CongregationRepository>();
        services.AddSingleton<ICongregationMapper, CongregationMapper>();

        // Assets
        services.AddScoped<AssetRepository>();
        services.AddScoped<AssetService>();
        services.AddSingleton<IAssetMapper, AssetMapper>();

        // Asset Categories
        services.AddScoped<AssetCategoryRepository>();
        services.AddScoped<AssetCategoryService>();
        services.AddSingleton<IAssetCategoryMapper, AssetCategoryMapper>();

        // Attendance
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<AttendanceService>();
        services.AddSingleton<IAttendanceMapper, AttendanceMapper>();

        // Attendance Types
        services.AddScoped<AttendanceTypeRepository>();
        services.AddScoped<AttendanceTypeService>();
        services.AddSingleton<IAttendanceTypeMapper, AttendanceTypeMapper>();

        // Attendance Contexts
        services.AddScoped<AttendanceContextRepository>();
        services.AddScoped<AttendanceContextService>();
        services.AddSingleton<IAttendanceContextMapper, AttendanceContextMapper>();

        // Events
        services.AddScoped<EventRepository>();
        services.AddScoped<EventService>();
        services.AddSingleton<IEventMapper, EventMapper>();

        // Event Attendance
        services.AddScoped<EventAttendanceRepository>();
        services.AddScoped<EventAttendanceService>();
        services.AddSingleton<IEventAttendanceMapper, EventAttendanceMapper>();

        // Event Registrations
        services.AddScoped<EventRegistrationRepository>();
        services.AddScoped<EventRegistrationService>();
        services.AddSingleton<IEventRegistrationMapper, EventRegistrationMapper>();

        // Organizations
        services.AddScoped<OrganizationRepository>();
        services.AddScoped<OrganizationService>();
        services.AddSingleton<IOrganizationMapper, OrganizationMapper>();

        // Organization Members
        services.AddScoped<OrganizationMemberRepository>();
        services.AddScoped<OrganizationMemberService>();
        services.AddSingleton<IOrganizationMemberMapper, OrganizationMemberMapper>();

        // Projects
        services.AddScoped<ProjectRepository>();
        services.AddScoped<ProjectService>();
        services.AddSingleton<IProjectMapper, ProjectMapper>();

        // Project Categories
        services.AddScoped<ProjectCategoryRepository>();
        services.AddScoped<ProjectCategoryService>();
        services.AddSingleton<IProjectCategoryMapper, ProjectCategoryMapper>();

        // Project Contributions
        services.AddScoped<ProjectContributionRepository>();
        services.AddScoped<ProjectContributionService>();
        services.AddSingleton<IProjectContributionMapper, ProjectContributionMapper>();

        // Tithes
        services.AddScoped<TitheRepository>();
        services.AddScoped<TitheService>();
        services.AddSingleton<ITitheMapper, TitheMapper>();

        // Transactions
        services.AddScoped<TransactionRepository>();
        services.AddScoped<TransactionService>();
        services.AddSingleton<ITransactionMapper, TransactionMapper>();

        // Transaction Categories
        services.AddScoped<TransactionCategoryRepository>();
        services.AddScoped<TransactionCategoryService>();
        services.AddSingleton<ITransactionCategoryMapper, TransactionCategoryMapper>();

        return services;
    }
}
