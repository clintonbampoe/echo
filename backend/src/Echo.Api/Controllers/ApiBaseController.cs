using Asp.Versioning;
using Echo.Application.HttpResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:ApiVersion}/[controller]")]
public abstract class ApiBaseController : ControllerBase
{
    protected Guid GetCongregationId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "CongregationId")?.Value;
        if (Guid.TryParse(claim, out var congregationId))
        {
            return congregationId;
        }
        throw new UnauthorizedAccessException("Congregation ID is missing from token.");
    }
}
