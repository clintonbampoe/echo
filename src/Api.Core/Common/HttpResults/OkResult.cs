using Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Common.HttpResults;

public class OkResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new OkObjectResult(new { message });
}
