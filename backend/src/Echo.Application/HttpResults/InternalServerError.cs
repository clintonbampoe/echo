using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class InternalServerError : IOperationResult
{
    private readonly string _detail;

    public InternalServerError()
    {
        _detail = "Operation Failed. Something went wrong while processing your request.";
    }

    public InternalServerError(string detail)
    {
        _detail = detail;
    }

    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = _detail,
            }
        )
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
}
