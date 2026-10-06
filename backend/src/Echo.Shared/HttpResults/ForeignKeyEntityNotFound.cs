using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class ForeignKeyEntityNotFound(string name) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = ErrorTypes.ForeignKeyNotFound,
                Title = "Related Resource Not Found",
                Detail = $"The referenced {name} does not exist or has been deleted.",
                Extensions = { ["errorCode"] = "FOREIGN_KEY_NOT_FOUND" },
            }
        )
        {
            StatusCode = StatusCodes.Status404NotFound,
        };
}
