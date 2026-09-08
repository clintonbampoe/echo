namespace Echo.Api.Extensions;

public static class FrontendClientConfigurationExtensions
{
    public static string ValidateFrontendClientBaseUrl(
        this IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        const string sectionName = "FrontendClient";
        var frontendBaseUrl = configuration[sectionName + ":BaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            throw new InvalidOperationException(
                "FrontendClient:BaseUrl must be set. Outbound emails depend on it.");
        }

        if (!Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var parsed)
            || (parsed.Scheme != "http" && parsed.Scheme != "https")
            || (parsed.Scheme != "https" && !environment.IsDevelopment()))
        {
            throw new InvalidOperationException(
                $"FrontendClient:BaseUrl must be an absolute http(s) URL (and https in non-dev). Got: {frontendBaseUrl}");
        }

        return frontendBaseUrl.TrimEnd('/');
    }
}