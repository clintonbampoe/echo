using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class UserNotVerifiedResult(string detail = "Verify your email before logging in.")
    : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status403Forbidden,
            title: "Account Email Unverified",
            type: ErrorTypes.EmailNotVerified
        );
}
