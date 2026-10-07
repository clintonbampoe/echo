using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class PagedResult<T>(PagedResponse<T> response) : IOperationResult
{
    public IResult ToResult() => TypedResults.Ok(response);
}
