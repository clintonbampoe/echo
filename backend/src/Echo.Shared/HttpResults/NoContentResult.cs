using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class NoContentResult : IOperationResult
{
    public IResult ToResult() => TypedResults.NoContent();
}
