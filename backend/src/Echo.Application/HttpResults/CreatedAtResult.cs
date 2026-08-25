using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class CreatedAtResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status201Created,
                Title = "Resource created successfully",
                Detail = message,
            }
        )
        {
            StatusCode = StatusCodes.Status201Created,
        };
}
