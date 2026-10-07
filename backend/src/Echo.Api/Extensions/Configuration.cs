using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using Echo.Shared.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Echo.Api.Extensions;

public static class Configuration
{
    public static IServiceCollection ConfigureApiRouting(this IServiceCollection services)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
        return services;
    }

    public static IServiceCollection ConfigureJsonSerializer(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    public static IServiceCollection ConfigureControllerOptions(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
            );
        return services;
    }

    public static IServiceCollection ConfigureSwaggerDocs(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseSwaggerUi(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant()
                );
            }
        });
        return app;
    }

    public static WebApplication UseScalarUi(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
            {
                options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
            })
            .AllowAnonymous();

        return app;
    }
}

internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "Echo API",
                    Version = description.ApiVersion.ToString(),
                    Description = $"""
                    Echo is a multi-tenant congregation management platform for managing members, attendance, finances, events, and organizational structure.

                    ## Authentication
                    All endpoints require a valid RS256 JWT Bearer token except Auth endpoints. Obtain a token via `POST /api/v1/auth/login` and pass it as `Authorization: Bearer <token>` on all subsequent requests. Access tokens are short-lived — use `POST /api/v1/auth/refresh` to obtain a new pair without re-authenticating.

                    ## Congregation Scoping
                    Every request is automatically scoped to the authenticated user's congregation via claims embedded in the JWT. There is no need to pass a congregation identifier — it is enforced server-side and cannot be overridden.

                    ## Pagination
                    List endpoints use cursor-based pagination. Responses include `hasMore` (boolean) and `next` (opaque cursor string). Pass `next` as the `cursor` query parameter on the subsequent request to fetch the next page. When `hasMore` is false, `next` is null and no further pages exist. Default page size is 24, maximum is 24.

                    ## Search
                    Search endpoints use trigram-based similarity matching (`pg_trgm`) and accept a `q` query parameter. Results are ranked by similarity. Search endpoints are rate-limited.

                    ## Soft Deletes
                    All delete operations are soft deletes — records are marked as deleted but not removed from the database. Soft-deleted records are excluded from all list, search, and lookup results automatically.

                    ## Error Responses
                    All error responses conform to RFC 7807 Problem Details and include a machine-readable `errorCode` extension field for client-side conditional handling.

                    {ErrorCatalog.ToMarkdownTable()}
                    """,
                }
            );
        }

        options.DocumentFilter<TagDescriptionDocumentFilter>();
    }
}

internal sealed class TagDescriptionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new HashSet<OpenApiTag>
        {
            new()
            {
                Name = "Users",
                Description =
                    "Users are accounts that have access to the Echo platform for a given congregation. A user has a role that determines their permissions across the system. Users are managed by congregation administrators and are scoped to the congregation.",
            },
            new()
            {
                Name = "Members",
                Description =
                    "Members are the core people record in Echo. A Member represents a fully registered individual within the congregation — they have a complete profile including personal details, contact information, next of kin, and membership status. Members are distinct from Visitors: a Visitor becomes a Member through an explicit conversion action. Member records drive attendance tracking, organization membership, and reporting.",
            },
            new()
            {
                Name = "Visitors",
                Description =
                    "Visitors represent individuals who have attended congregation activities but are not yet registered as full Members. A Visitor record captures basic contact and attendance context. Visitors can be promoted to full Members via the convert endpoint, which creates a complete Member profile from the Visitor record. Visitor records are scoped to the congregation.",
            },
            new()
            {
                Name = "Attendance",
                Description =
                    "Attendance records track member presence at congregation services and activities. Each record links a member to an attendance type on a specific date. Attendance drives the summary metrics used for reporting and trend analysis. Records support filtering by date range, member, and attendance type.",
            },
            new()
            {
                Name = "Attendance Types",
                Description =
                    "Attendance Types are congregation-defined categories of service or activity — for example Sunday Service, Midweek Service, or Prayer Meeting. Every attendance record must reference an attendance type. Types are lightweight lookup entities; the full list is always returned without pagination.",
            },
            new()
            {
                Name = "Events",
                Description =
                    "Events represent scheduled congregation activities such as conferences, outreaches, and special services. An event has a name, date range, and optional capacity. Events support registration and attendance tracking through their respective sub-resources. Events are scoped to the congregation.",
            },
            new()
            {
                Name = "Event Attendance",
                Description =
                    "Event Attendance records track which members actually attended a specific event. Distinct from Event Registrations — a member may register but not attend, or attend without prior registration. Records can be queried by event or by member.",
            },
            new()
            {
                Name = "Event Registrations",
                Description =
                    "Event Registrations capture member sign-ups for upcoming events. Registration does not imply attendance — attendance is tracked separately via the Event Attendance resource. Records can be queried by event or by member.",
            },
            new()
            {
                Name = "Organizations",
                Description =
                    "Organizations are internal congregation groups such as departments, ministries, choirs, and committees. A congregation can have multiple organizations and members can belong to more than one. Organizations support member assignment through Organization Members and have their own summary metrics.",
            },
            new()
            {
                Name = "Organization Members",
                Description =
                    "Organization Members represent the membership link between a Member and an Organization. A record captures the member, the organization, their role within it, and the date they joined. Records can be queried by member or by organization.",
            },
            new()
            {
                Name = "Assets",
                Description =
                    "Assets track physical and non-physical resources owned or managed by the congregation — equipment, property, vehicles, and other inventory. Each asset belongs to an Asset Category and carries a purchase cost and current value supporting depreciation tracking. Assets are scoped to the congregation.",
            },
            new()
            {
                Name = "Asset Categories",
                Description =
                    "Asset Categories are congregation-defined classifications used to group and filter assets. Examples include Equipment, Furniture, and Vehicles. Every asset must belong to a category. Categories are lightweight lookup entities; the full list is always returned without pagination.",
            },
            new()
            {
                Name = "Projects",
                Description =
                    "Projects represent congregation fundraising or development initiatives — building projects, outreach campaigns, equipment drives. A project has a funding goal, timeline, and category. Contributions are tracked as a separate sub-resource. Projects are scoped to the congregation.",
            },
            new()
            {
                Name = "Project Categories",
                Description =
                    "Project Categories are congregation-defined classifications for projects — for example Construction, Outreach, or Equipment. Every project must belong to a category. Categories are lightweight lookup entities; the full list is always returned without pagination.",
            },
            new()
            {
                Name = "Project Contributions",
                Description =
                    "Project Contributions record individual financial or in-kind contributions made toward a specific project. Each contribution links a member to a project with an amount and date. Contributions drive the project summary metrics. Scoped to the congregation.",
            },
            new()
            {
                Name = "Tithes",
                Description =
                    "Tithes record individual tithe payments made by members. Each record captures the member, amount, payment date, and payment method. Tithes are distinct from general transactions and have their own summary metrics for financial reporting. Scoped to the congregation.",
            },
            new()
            {
                Name = "Transactions",
                Description =
                    "Transactions record general financial activity for the congregation — income and expenditure outside of tithes and project contributions. Each transaction belongs to a Transaction Category and carries an amount, date, type, and optional notes. Transactions support summary metrics for financial reporting. Scoped to the congregation.",
            },
            new()
            {
                Name = "Transaction Categories",
                Description =
                    "Transaction Categories are congregation-defined classifications for financial transactions — for example Offerings, Utilities, or Salaries. Every transaction must belong to a category. Categories are lightweight lookup entities; the full list is always returned without pagination.",
            },
        };
    }
}
