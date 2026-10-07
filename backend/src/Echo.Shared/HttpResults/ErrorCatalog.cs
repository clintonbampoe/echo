namespace Echo.Shared.HttpResults;

public static class ErrorCatalog
{
    public static readonly IReadOnlyList<ErrorEntry> Entries =
    [
        new("400", "VALIDATION_ERROR", "Field validation failure — see `errors` object"),
        new("400", "BAD_REQUEST", "Malformed request body"),
        new("401", "INVALID_CREDENTIALS", "Wrong email or password"),
        new("401", "INVALID_TOKEN", "Token invalid or already used"),
        new("403", "EMAIL_NOT_VERIFIED", "Account exists but email unverified"),
        new("404", "NOT_FOUND", "Entity does not exist or has been soft-deleted"),
        new("404", "FOREIGN_KEY_NOT_FOUND", "Referenced entity does not exist"),
        new("409", "CONFLICT", "Duplicate or state collision"),
        new("500", "INTERNAL_SERVER_ERROR", "Unhandled server exception"),
    ];

    public static string ToMarkdownTable()
    {
        var rows = Entries.Select(e => $"| {e.Status} | `{e.ErrorCode}` | {e.Cause} |");
        return $"""
            | Status | errorCode | Cause |
            |--------|-----------|-------|
            {string.Join("\n            ", rows)}
            """;
    }
}

public record ErrorEntry(string Status, string ErrorCode, string Cause);
