using Microsoft.AspNetCore.Mvc;

namespace  Api.Core.Common.HttpResults.Interfaces;

public interface IOperationResult
{
    ActionResult ToActionResult();
}

