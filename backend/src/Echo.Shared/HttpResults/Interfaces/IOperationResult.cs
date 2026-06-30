using Microsoft.AspNetCore.Mvc;

namespace  Echo.Shared.HttpResults.Interfaces;

public interface IOperationResult
{
    ActionResult ToActionResult();
}

