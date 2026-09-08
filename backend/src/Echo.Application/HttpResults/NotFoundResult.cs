using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class NotFoundResult(string id) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "The requested resource was not found",
                Detail = $"The resource with Id: {id} is invalid or has been deleted.",
            }
        )
        {
            StatusCode = StatusCodes.Status404NotFound,
        };
}
