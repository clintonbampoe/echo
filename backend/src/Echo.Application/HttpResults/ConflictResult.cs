using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class ConflictResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new ConflictObjectResult(new { error = message });
}
