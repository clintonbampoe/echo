using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class BadRequestResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new BadRequestObjectResult(new { error = message });
}
