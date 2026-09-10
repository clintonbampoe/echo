using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class ForeignKeyEntityNotFound(string name) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "The requested resource was not found",
                Detail =
                    $"The foreign key property specified with Name ({name})  is invalid or has been deleted.",
            }
        )
        {
            StatusCode = StatusCodes.Status404NotFound,
        };
}
