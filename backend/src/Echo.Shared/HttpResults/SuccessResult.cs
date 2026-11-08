using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class SuccessResult<T>(T data) : IOperationResult
{
    public IResult ToResult() => TypedResults.Ok(data);
}

public static class SuccessResult
{
    public static SuccessResult<T> Of<T>(T data) => new(data);
}
