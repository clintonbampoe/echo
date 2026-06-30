using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class NotFoundResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new NotFoundObjectResult(new { error = message });
}
