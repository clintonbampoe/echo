using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class BadRequestResult(string detail) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ErrorTypes.BadRequest,
                Title = "Bad Request",
                Detail = detail,
                Extensions = { ["errorCode"] = "BAD_REQUEST" },
            }
        )
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };
}
