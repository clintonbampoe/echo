using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class NotFoundResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new NotFoundObjectResult(new { error = message });
}
