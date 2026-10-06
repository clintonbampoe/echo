using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class NotFoundResult(string id) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = ErrorTypes.NotFound,
                Title = "Resource Not Found",
                Detail = $"The resource with Id: {id} was not found or has been deleted.",
                Extensions = { ["errorCode"] = "NOT_FOUND" },
            }
        )
        {
            StatusCode = StatusCodes.Status404NotFound,
        };
}
