using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssetsController : ControllerBase
{
    [HttpGet]
    public Task<ActionResult> GetPageAsync(
        CancellationToken ct,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters
    )
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    public Task<ActionResult> GetByIdAsync(
        Guid id,
        CancellationToken ct
    )
    {
        throw new NotImplementedException();
    }

}
