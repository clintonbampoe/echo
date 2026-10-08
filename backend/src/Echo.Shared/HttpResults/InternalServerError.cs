using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class InternalServerError(
    string detail = "Something went wrong while processing your request."
) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error",
            type: ErrorTypes.InternalServerError
        );
}
