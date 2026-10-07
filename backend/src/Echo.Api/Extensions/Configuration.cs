using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
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
