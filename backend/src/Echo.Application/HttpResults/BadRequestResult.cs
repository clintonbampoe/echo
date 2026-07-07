using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class BadRequestResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new BadRequestObjectResult(new { error = message });
}
