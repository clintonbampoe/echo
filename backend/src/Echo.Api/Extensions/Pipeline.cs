namespace Echo.Api.Extensions;

public static class Pipeline
{
    /// <summary>
    /// The request pipeline, in the order it must run. Keep the order: forwarded headers come
    /// first so everything after sees the real client address and scheme (the rate limiter
    /// buckets by it), and the rate limiter runs after authentication.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseSecurityHeaders();
        app.ConfigureExceptionHandler();
        app.UseRouting();
        app.UseCors(Cors.FrontendPolicy);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        return app;
    }
}
