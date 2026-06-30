using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class OkResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new OkObjectResult(new { message });
}
