using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult
{
    public ActionResult ToActionResult() => new OkObjectResult(data);
}
