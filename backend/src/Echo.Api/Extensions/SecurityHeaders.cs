namespace Echo.Api.Extensions;

public static class SecurityHeaders
{
    internal static void Set(IHeaderDictionary headers, bool isDevelopment)
    {
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // The API only returns JSON, so nothing may be loaded or framed. The development API docs
        // (Swagger, Scalar) load scripts, so the policy is left out there.
        if (!isDevelopment)
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        // No Strict-Transport-Security here: browsers only honour it over HTTPS and it is sticky,
        // so it belongs in the proxy that terminates TLS (see docs/Infrastructure.md).
    }

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        var isDevelopment = app.Environment.IsDevelopment();

        app.Use(
            (context, next) =>
            {
                // Set as the response starts: the exception handler clears headers, so setting
                // them earlier would lose them on error responses.
                context.Response.OnStarting(() =>
                {
                    Set(context.Response.Headers, isDevelopment);
                    return Task.CompletedTask;
                });
                return next(context);
            }
        );

        return app;
    }
}
