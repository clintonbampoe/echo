using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class BadRequestResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "An unexpected error occured with your request.",
                Detail = message,
            }
        )
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };
}
