using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class InternalServerError() : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "Operation Failed. Something went wrong while processing your request.",
            }
        )
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
}
