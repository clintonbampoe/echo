using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class OkResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status200OK,
                Title = "Operation completed successfully.",
                Detail = message,
            }
        )
        {
            StatusCode = StatusCodes.Status200OK,
        };
}
