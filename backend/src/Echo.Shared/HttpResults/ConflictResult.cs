using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class ConflictResult(string detail) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            type: ErrorTypes.Conflict
        );
}
