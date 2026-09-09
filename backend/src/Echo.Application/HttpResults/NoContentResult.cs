using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class NoContentResult : IOperationResult
{
    public ActionResult ToActionResult() =>
        new StatusCodeResult(StatusCodes.Status204NoContent);
}
