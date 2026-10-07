using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class CreatedResult<T>(string locationUri, T data) : IOperationResult
{
    public IResult ToResult() => TypedResults.Created(locationUri, data);
}
