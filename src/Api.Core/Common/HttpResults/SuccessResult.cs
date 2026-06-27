using Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Common.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult<T>
{
    public T? Data { get; } = data;
    public ActionResult ToActionResult() => new OkObjectResult(Data);
}
