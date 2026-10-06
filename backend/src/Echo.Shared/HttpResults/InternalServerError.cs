using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class InternalServerError(
    string detail = "Something went wrong while processing your request."
) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = ErrorTypes.InternalServerError,
                Title = "Internal Server Error",
                Detail = detail,
                Extensions = { ["errorCode"] = "INTERNAL_SERVER_ERROR" },
            }
        )
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
}
