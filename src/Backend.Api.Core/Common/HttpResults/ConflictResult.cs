using Backend.Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Common.HttpResults;

public class ConflictResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new ConflictObjectResult(new { error = message });
}
