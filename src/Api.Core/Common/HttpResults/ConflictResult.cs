using Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Common.HttpResults;

public class ConflictResult(string message) : IOperationResult
{
    public ActionResult ToActionResult() => new ConflictObjectResult(new { error = message });
}
