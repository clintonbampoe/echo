using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class NotFoundResult(string id) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: $"The resource with Id: {id} was not found or has been deleted.",
            statusCode: StatusCodes.Status404NotFound,
            title: "Resource Not Found",
            type: ErrorTypes.NotFound
        );
}
