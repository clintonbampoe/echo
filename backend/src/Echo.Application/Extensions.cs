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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Echo.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Person
        services.AddScoped<PersonRepository>();

        // Members
        services.AddScoped<MemberRepository>();
        services.AddScoped<MemberService>();
        services.AddSingleton<IMemberMapper, MemberMapper>();

        // Visitors
        services.AddScoped<VisitorRepository>();
        services.AddScoped<VisitorService>();
        services.AddSingleton<IVisitorMapper, VisitorMapper>();

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

        services.AddSingleton<ApplicationInstrumentation>();
        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                ApplicationInstrumentation.ConfigureTracing(tracing);
                tracing.AddEntityFrameworkCoreInstrumentation();
            })
            .WithMetrics(metrics => ApplicationInstrumentation.ConfigureMetrics(metrics));

        return services;
    }

    public static RouteGroupBuilder MapApplicationEndpoints(
        this RouteGroupBuilder group,
        ApplicationInstrumentation instrumentation
    )
    {
        group.MapAssetEndpoints(instrumentation);
        group.MapAssetCategoryEndpoints(instrumentation);
        group.MapAttendanceEndpoints(instrumentation);
        group.MapAttendanceTypeEndpoints(instrumentation);
        group.MapEventEndpoints(instrumentation);
        group.MapEventAttendanceEndpoints(instrumentation);
        group.MapEventRegistrationEndpoints(instrumentation);
        group.MapMemberEndpoints(instrumentation);
        group.MapVisitorEndpoints(instrumentation);
        group.MapOrganizationEndpoints(instrumentation);
        group.MapOrganizationMemberEndpoints(instrumentation);
        group.MapProjectEndpoints(instrumentation);
        group.MapProjectCategoryEndpoints(instrumentation);
        group.MapProjectContributionEndpoints(instrumentation);
        group.MapTitheEndpoints(instrumentation);
        group.MapTransactionEndpoints(instrumentation);
        group.MapTransactionCategoryEndpoints(instrumentation);
        group.MapUserEndpoints(instrumentation);

        return group;
    }
}
