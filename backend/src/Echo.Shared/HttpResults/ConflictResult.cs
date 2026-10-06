using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class ConflictResult(string detail) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = ErrorTypes.Conflict,
                Title = "Conflict",
                Detail = detail,
                Extensions = { ["errorCode"] = "CONFLICT" },
            }
        )
        {
            StatusCode = StatusCodes.Status409Conflict,
        };
}
