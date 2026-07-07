using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class OkResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new OkObjectResult(new { message });
}
