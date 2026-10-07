using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public class ForeignKeyEntityNotFound(string name) : IOperationResult
{
    public IResult ToResult() =>
        TypedResults.Problem(
            detail: $"The referenced {name} does not exist or has been deleted.",
            statusCode: StatusCodes.Status404NotFound,
            title: "Related Resource Not Found",
            type: ErrorTypes.ForeignKeyNotFound
        );
}
