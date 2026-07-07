using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult<T>
{
    public T? Data { get; } = data;
    public ActionResult ToActionResult() => new OkObjectResult(Data);
}
