using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers.Base;

[ApiController]
public abstract class BaseController : ControllerBase
{
    // Temporary until JWT auth is implemented.
    // CongregationId will come from the token claim instead of a header.
    protected Guid GetCongregationId()
    {
        var header = HttpContext.Request.Headers["X-Congregation-Id"].ToString();

        if (!Guid.TryParse(header, out var congregationId))
            throw new InvalidOperationException("X-Congregation-Id header is missing or invalid.");

        return congregationId;
    }
}
