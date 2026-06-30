using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult<T>
{
    public T? Data { get; } = data;
    public ActionResult ToActionResult() => new OkObjectResult(Data);
}
