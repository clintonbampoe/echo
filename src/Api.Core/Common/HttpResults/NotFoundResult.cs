using Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Common.HttpResults;

public class NotFoundResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new NotFoundObjectResult(new { error = message });
}
