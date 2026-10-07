using Asp.Versioning;
using Echo.Shared.Options.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Echo.Api.Extensions;

public static class Api
{
    public static IServiceCollection ConfigureApiOptions(this IServiceCollection services)
    {
        services
            .AddOptions<ApiOptions>()
            .BindConfiguration(ApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection ConfigureApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    public static IServiceCollection ConfigureInvalidModelStateResponse(
        this IServiceCollection services
    )
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var apiOptions = context
                    .HttpContext.RequestServices.GetRequiredService<IOptions<ApiOptions>>()
                    .Value;

                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Type = $"{apiOptions.BaseUrl}/errors/validation-error",
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details.",
                    Instance = context.HttpContext.Request.Path,
                };

                problem.Extensions["errorCode"] = "VALIDATION_ERROR";

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" },
                };
            };
        });

        return services;
    }
}
