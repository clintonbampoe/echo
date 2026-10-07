using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult
{
    public IResult ToResult() => TypedResults.Ok(data);
}
