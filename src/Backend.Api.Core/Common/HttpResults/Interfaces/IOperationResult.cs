using Microsoft.AspNetCore.Mvc;

namespace  Backend.Api.Core.Common.HttpResults.Interfaces;

public interface IOperationResult
{
    ActionResult ToActionResult();
}

