namespace Echo.Shared.HttpResults;

/// <summary>
/// The "type" URIs of the API's problem responses. They start from the API's base URL, which the
/// API passes to <see cref="UseBaseUrl"/> once at startup from its validated options. Nothing in
/// here reads the environment.
/// </summary>
public static class ErrorTypes
{
    private static string _base = "urn:echo:errors";

    public static void UseBaseUrl(string apiBaseUrl) => _base = $"{apiBaseUrl.TrimEnd('/')}/errors";

    public static string ValidationError => $"{_base}/validation-error";
    public static string BadRequest => $"{_base}/bad-request";
    public static string InvalidCredentials => $"{_base}/invalid-credentials";
    public static string InvalidToken => $"{_base}/invalid-token";
    public static string EmailNotVerified => $"{_base}/email-not-verified";
    public static string NotFound => $"{_base}/not-found";
    public static string ForeignKeyNotFound => $"{_base}/foreign-key-not-found";
    public static string Conflict => $"{_base}/conflict";
    public static string InternalServerError => $"{_base}/internal-server-error";
}
