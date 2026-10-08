using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class InvalidTokenResult : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: "The token is invalid or has already been used.",
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Invalid Token",
            type: ErrorTypes.InvalidToken
        );
}
