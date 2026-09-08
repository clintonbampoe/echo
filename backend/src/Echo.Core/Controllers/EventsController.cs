using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers;

public class EventsController(EventService service) : CoreBaseController
{
    [HttpGet("summary")]
    public async Task<ActionResult> GetSummary(CancellationToken ct)
    {
        var response = await service.GetSummary(GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet]
    public async Task<ActionResult> GetPage(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var response = await service.GetPage(
            GetCongregationId(),
            paginationParameters,
            queryParameters,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(EventCreateDto dto, CancellationToken ct)
    {
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, EventUpdateDto dto, CancellationToken ct)
    {
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var response = await service.Delete(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }
}
