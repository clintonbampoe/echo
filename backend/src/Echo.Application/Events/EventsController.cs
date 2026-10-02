using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Application.Events;

public class EventsController(EventService service, ApplicationInstrumentation instrumentation)
    : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] EventFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.list");
        var response = await service.List(GetCongregationId(), filters, pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.fetch_by_id");
        var response = await service.GetById(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(EventCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, EventUpdateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.delete");
        var response = await service.Delete(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event.search");
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }
}
