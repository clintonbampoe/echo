using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class CreatedResult<T>(string locationUri, T data) : IOperationResult
{
    public ActionResult ToActionResult() => new CreatedResult(locationUri, data);
}
