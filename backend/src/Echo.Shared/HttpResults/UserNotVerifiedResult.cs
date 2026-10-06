using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class UserNotVerifiedResult(string detail = "Verify your email before logging in.")
    : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = ErrorTypes.EmailNotVerified,
                Title = "Account Email Unverified",
                Detail = detail,
                Extensions = { ["errorCode"] = "EMAIL_NOT_VERIFIED" },
            }
        )
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
}
