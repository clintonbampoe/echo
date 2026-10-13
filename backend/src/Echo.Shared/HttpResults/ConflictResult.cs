using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

/// <param name="detail">What conflicts, in words for the user.</param>
/// <param name="errorCode">An optional stable code (for example EVENT_FULL) that clients can branch on.</param>
public class ConflictResult(string detail, string? errorCode = null) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            type: ErrorTypes.Conflict,
            extensions: errorCode is null
                ? null
                : new Dictionary<string, object?> { ["errorCode"] = errorCode }
        );
}
