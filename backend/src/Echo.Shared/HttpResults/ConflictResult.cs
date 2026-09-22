using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class ConflictResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "An unexpected error occured with your request.",
                Detail = message,
            }
        )
        {
            StatusCode = StatusCodes.Status409Conflict,
        };
}
