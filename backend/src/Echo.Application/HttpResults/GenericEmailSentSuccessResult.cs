using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class GenericEmailSentSuccessResult : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status200OK,
                Title = "Operation completed successfully.",
                Detail = "You'll receive an email shortly if account exists",
            }
        )
        {
            StatusCode = StatusCodes.Status200OK,
        };
}
