using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class PagedResult<T>(PagedResponse<T> response) : IOperationResult
{
    public ActionResult ToActionResult() => new OkObjectResult(response);
}
