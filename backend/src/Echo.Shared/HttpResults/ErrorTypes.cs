namespace Echo.Shared.HttpResults;

public static class ErrorTypes
{
    private static readonly string _base = BuildBaseUrl();

    private static string BuildBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("Api__BaseUrl");

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "CRITICAL: Environment variable 'Api__BaseUrl' is missing or empty. "
                    + "You must specify 'Api__BaseUrl' (e.g., 'http://localhost:5025' or 'https://api.echo.com') to launch the application."
            );
        }

        return $"{baseUrl.TrimEnd('/')}/errors";
    }

    public static readonly string ValidationError = $"{_base}/validation-error";
    public static readonly string BadRequest = $"{_base}/bad-request";
    public static readonly string InvalidCredentials = $"{_base}/invalid-credentials";
    public static readonly string InvalidToken = $"{_base}/invalid-token";
    public static readonly string EmailNotVerified = $"{_base}/email-not-verified";
    public static readonly string NotFound = $"{_base}/not-found";
    public static readonly string ForeignKeyNotFound = $"{_base}/foreign-key-not-found";
    public static readonly string Conflict = $"{_base}/conflict";
    public static readonly string InternalServerError = $"{_base}/internal-server-error";
}
