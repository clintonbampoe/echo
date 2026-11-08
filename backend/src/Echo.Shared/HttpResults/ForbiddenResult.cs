using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

/// <param name="detail">What is refused, in words for the user.</param>
/// <param name="errorCode">A stable code (for example ACCOUNT_DISABLED) that clients can branch on.</param>
public class ForbiddenResult(string detail, string errorCode) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            type: ErrorTypes.Forbidden,
            extensions: new Dictionary<string, object?> { ["errorCode"] = errorCode }
        );
}
