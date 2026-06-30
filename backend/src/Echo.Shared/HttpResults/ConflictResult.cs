using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class ConflictResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new ConflictObjectResult(new { error = message });
}
