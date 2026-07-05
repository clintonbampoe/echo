using Microsoft.AspNetCore.Mvc;

namespace  Echo.Application.HttpResults;

public interface IOperationResult
{
    ActionResult ToActionResult();
}

