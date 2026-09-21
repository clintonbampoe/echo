using Microsoft.AspNetCore.Mvc;

namespace  Echo.Shared.HttpResults;

public interface IOperationResult
{
    ActionResult ToActionResult();
}

