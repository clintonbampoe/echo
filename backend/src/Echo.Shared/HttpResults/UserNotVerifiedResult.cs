using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class UserNotVerifiedResult(string message = "Verify your email before logging in.")
    : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://yourdomain.com/errors/email-not-verified",
                Title = "Account Email Unverified",
                Detail = message,
                Extensions = { ["errorCode"] = "EMAIL_NOT_VERIFIED" },
            }
        )
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
}
