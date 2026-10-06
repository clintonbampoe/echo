using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class InvalidTokenResult : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = ErrorTypes.InvalidToken,
                Title = "Invalid Token",
                Detail = "The token is invalid or has already been used.",
                Extensions = { ["errorCode"] = "INVALID_TOKEN" },
            }
        )
        {
            StatusCode = StatusCodes.Status401Unauthorized,
        };
}
