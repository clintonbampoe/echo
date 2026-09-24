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
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Echo.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Members
        services.AddScoped<MemberRepository>();
        services.AddScoped<MemberService>();
        services.AddScoped<MemberController>();
        services.AddSingleton<IMemberMapper, MemberMapper>();

        // Users
        services.AddScoped<UserRepository>();
        services.AddScoped<UserService>();
        services.AddSingleton<IUserMapper, UserMapper>();

        // Congregations
        services.AddScoped<CongregationRepository>();
        services.AddSingleton<ICongregationMapper, CongregationMapper>();

        // Assets
        services.AddScoped<AssetRepository>();
        services.AddScoped<AssetService>();
        services.AddScoped<AssetController>();
        services.AddSingleton<IAssetMapper, AssetMapper>();

        // Asset Categories
        services.AddScoped<AssetCategoryRepository>();
        services.AddScoped<AssetCategoryService>();
        services.AddScoped<AssetCategoryController>();
        services.AddSingleton<IAssetCategoryMapper, AssetCategoryMapper>();

        // Attendance
        services.AddScoped<AttendanceRepository>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<AttendanceController>();
        services.AddSingleton<IAttendanceMapper, AttendanceMapper>();

        // Attendance Types
        services.AddScoped<AttendanceTypeRepository>();
        services.AddScoped<AttendanceTypeService>();
        services.AddScoped<AttendanceTypeController>();
        services.AddSingleton<IAttendanceTypeMapper, AttendanceTypeMapper>();

        // Attendance Contexts
        services.AddScoped<AttendanceContextRepository>();
        services.AddScoped<AttendanceContextService>();
        services.AddScoped<AttendanceContextsController>();
        services.AddSingleton<IAttendanceContextMapper, AttendanceContextMapper>();

        // Events
        services.AddScoped<EventRepository>();
        services.AddScoped<EventService>();
        services.AddScoped<EventController>();
        services.AddSingleton<IEventMapper, EventMapper>();

        // Event Attendance
        services.AddScoped<EventAttendanceRepository>();
        services.AddScoped<EventAttendanceService>();
        services.AddScoped<EventAttendanceController>();
        services.AddSingleton<IEventAttendanceMapper, EventAttendanceMapper>();

        // Event Registrations
        services.AddScoped<EventRegistrationRepository>();
        services.AddScoped<EventRegistrationService>();
        services.AddScoped<EventRegistrationsController>();
        services.AddSingleton<IEventRegistrationMapper, EventRegistrationMapper>();

        // Organizations
        services.AddScoped<OrganizationRepository>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<OrganizationController>();
        services.AddSingleton<IOrganizationMapper, OrganizationMapper>();

        // Organization Members
        services.AddScoped<OrganizationMemberRepository>();
        services.AddScoped<OrganizationMemberService>();
        services.AddScoped<OrganizationMemberController>();
        services.AddSingleton<IOrganizationMemberMapper, OrganizationMemberMapper>();

        // Projects
        services.AddScoped<ProjectRepository>();
        services.AddScoped<ProjectService>();
        services.AddScoped<ProjectController>();
        services.AddSingleton<IProjectMapper, ProjectMapper>();

        // Project Categories
        services.AddScoped<ProjectCategoryRepository>();
        services.AddScoped<ProjectCategoryService>();
        services.AddScoped<ProjectCategoryController>();
        services.AddSingleton<IProjectCategoryMapper, ProjectCategoryMapper>();

        // Project Contributions
        services.AddScoped<ProjectContributionRepository>();
        services.AddScoped<ProjectContributionService>();
        services.AddScoped<ProjectContributionController>();
        services.AddSingleton<IProjectContributionMapper, ProjectContributionMapper>();

        // Tithes
        services.AddScoped<TitheRepository>();
        services.AddScoped<TitheService>();
        services.AddScoped<TitheController>();
        services.AddSingleton<ITitheMapper, TitheMapper>();

        // Transactions
        services.AddScoped<TransactionRepository>();
        services.AddScoped<TransactionService>();
        services.AddScoped<TransactionController>();
        services.AddSingleton<ITransactionMapper, TransactionMapper>();

        // Transaction Categories
        services.AddScoped<TransactionCategoryRepository>();
        services.AddScoped<TransactionCategoryService>();
        services.AddScoped<TransactionCategoryController>();
        services.AddSingleton<ITransactionCategoryMapper, TransactionCategoryMapper>();

        services.AddSingleton<InstrumentationSource>();
        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                InstrumentationSource.ConfigureTracing(tracing);
                tracing.AddEntityFrameworkCoreInstrumentation();
            })
            .WithMetrics(metrics => InstrumentationSource.ConfigureMetrics(metrics));

        return services;
    }
}
