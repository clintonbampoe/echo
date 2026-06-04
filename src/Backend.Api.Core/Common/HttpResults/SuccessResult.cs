using Backend.Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Common.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult<T>
{
    public T? Data { get; } = data;
    public ActionResult ToActionResult() => new OkObjectResult(Data);
}
