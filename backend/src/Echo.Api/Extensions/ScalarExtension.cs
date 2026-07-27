using Scalar.AspNetCore;

namespace Echo.Api.Extensions;

public static class ScalarExtensions
{
    public static WebApplication UseScalarDocumentation(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
        }).AllowAnonymous();

        return app;
    }
}
