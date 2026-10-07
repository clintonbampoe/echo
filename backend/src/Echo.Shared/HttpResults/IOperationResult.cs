using Microsoft.AspNetCore.Http;

namespace Echo.Shared.HttpResults;

public interface IOperationResult
{
    IResult ToResult();
}
