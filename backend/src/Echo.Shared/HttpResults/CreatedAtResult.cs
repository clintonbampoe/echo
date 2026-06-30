using Echo.Shared.HttpResults.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Shared.HttpResults;

public class CreatedAtResult() : IOperationResult
{
    public ActionResult ToActionResult()
    {
        throw new NotImplementedException();
    }
}
