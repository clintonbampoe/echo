using Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Common.HttpResults;

public class BadRequestResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new BadRequestObjectResult(new { error = message });
}
