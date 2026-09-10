using Echo.Core.Mapping.AssetCategoryMapping;
using Echo.Core.Mapping.AssetMapping;
using Echo.Core.Mapping.AttendanceContextMapping;
using Echo.Core.Mapping.AttendanceMapping;
using Echo.Core.Mapping.AttendanceTypeMapping;
using Echo.Core.Mapping.CongregationMapping;
using Echo.Core.Mapping.EventAttendanceMapping;
using Echo.Core.Mapping.EventMapping;
using Echo.Core.Mapping.EventRegistrationMapping;
using Echo.Core.Mapping.MemberMapping;
using Echo.Core.Mapping.OrganizationMapping;
using Echo.Core.Mapping.OrganizationMemberMapping;
using Echo.Core.Mapping.ProjectCategoryMapping;
using Echo.Core.Mapping.ProjectContributionMapping;
using Echo.Core.Mapping.ProjectMapping;
using Echo.Core.Mapping.TitheMapping;
using Echo.Core.Mapping.TransactionCategoryMapping;
using Echo.Core.Mapping.TransactionMapping;
using Echo.Core.Mapping.UserMapping;
using Echo.Core.Repositories;
using Echo.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Core.Extensions;

public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
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
        services.AddScoped<CongregationRepository>();

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

        services.AddSingleton<IAssetCategoryMapper, AssetCategoryMapper>();
        services.AddSingleton<IAssetMapper, AssetMapper>();
        services.AddSingleton<IAttendanceTypeMapper, AttendanceTypeMapper>();
        services.AddSingleton<IAttendanceContextMapper, AttendanceContextMapper>();
        services.AddSingleton<IAttendanceMapper, AttendanceMapper>();
        services.AddSingleton<ICongregationMapper, CongregationMapper>();
        services.AddSingleton<IMemberMapper, MemberMapper>();
        services.AddSingleton<IUserMapper, UserMapper>();
        services.AddSingleton<IEventMapper, EventMapper>();
        services.AddSingleton<IEventRegistrationMapper, EventRegistrationMapper>();
        services.AddSingleton<IEventAttendanceMapper, EventAttendanceMapper>();
        services.AddSingleton<ICongregationMapper, CongregationMapper>();
        services.AddSingleton<IMemberMapper, MemberMapper>();
        services.AddSingleton<IOrganizationMapper, OrganizationMapper>();
        services.AddSingleton<IOrganizationMemberMapper, OrganizationMemberMapper>();
        services.AddSingleton<ITitheMapper, TitheMapper>();
        services.AddSingleton<ITransactionMapper, TransactionMapper>();
        services.AddSingleton<ITransactionCategoryMapper, TransactionCategoryMapper>();
        services.AddSingleton<IProjectMapper, ProjectMapper>();
        services.AddSingleton<IProjectCategoryMapper, ProjectCategoryMapper>();
        services.AddSingleton<IProjectContributionMapper, ProjectContributionMapper>();

        return services;
    }
}
