namespace Echo.Api.Extensions;

public static class Cors
{
    public const string FrontendPolicy = "FrontendPolicy";

    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var allowedOrigins = GetAllowedOrigins(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(
                FrontendPolicy,
                policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            );
        });
        return services;
    }

    private static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var originsString =
            configuration["Cors:AllowedOrigins"]
            ?? throw new InvalidOperationException(
                "CORS configuration missing: 'Cors:AllowedOrigins' is required."
            );

        return originsString.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
    }
}
