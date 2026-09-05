using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class CreatedAtResult<T>(T resource) : IOperationResult
{
    public ActionResult ToActionResult() =>
        new ObjectResult(
            new
            {
                Status = StatusCodes.Status201Created,
                Title = "Resource created successfully",
                Resource = resource,
            }
        )
        {
            StatusCode = StatusCodes.Status201Created,
        };
}
