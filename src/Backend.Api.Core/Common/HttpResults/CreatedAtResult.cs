using Backend.Api.Core.Common.HttpResults.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Common.HttpResults;

public class CreatedAtResult() : IOperationResult
{
    public ActionResult ToActionResult()
    {
        throw new NotImplementedException();
    }
}
