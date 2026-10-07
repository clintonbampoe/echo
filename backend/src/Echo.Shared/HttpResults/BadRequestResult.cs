using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class BadRequestResult(string detail) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Bad Request",
            type: ErrorTypes.BadRequest
        );
}
