using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.HttpResults;

public class CreatedAtResult() : IOperationResult
{
    public ActionResult ToActionResult()
    {
        throw new NotImplementedException();
    }
}
