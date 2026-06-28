using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Controllers.Base;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Controllers;

[Route("api/[controller]")]
public class EventsController(EventService service) : BaseController
{
    private readonly EventService _service = service;

    [HttpGet]
    public async Task<ActionResult> GetPageAsync(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var response = await _service.GetPageAsync(
            GetCongregationId(),
            paginationParameters,
            queryParameters,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(EventCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, EventUpdateDto dto, CancellationToken ct)
    {
        var response = await _service.UpdateAsync(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);
        return response.ToActionResult();
    }
}
