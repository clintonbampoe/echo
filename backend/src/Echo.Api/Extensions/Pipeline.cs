namespace Echo.Api.Extensions;

public static class Pipeline
{
    /// <summary>
    /// The request pipeline, in the order it must run. Keep the order: forwarded headers come
    /// first so everything after sees the real client address and scheme (the rate limiter
    /// buckets by it), the rate limiter runs after authentication, and the output cache after both, so a
    /// cached answer is only given to someone who is signed in and still counts toward their limit.
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
        app.UseOutputCache();

        return app;
    }
}
