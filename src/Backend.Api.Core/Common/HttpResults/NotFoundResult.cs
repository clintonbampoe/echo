using Backend.Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Common.HttpResults;

public class NotFoundResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new NotFoundObjectResult(new { error = message });
}
