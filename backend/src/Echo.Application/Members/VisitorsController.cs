using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Application.Members;

public class VisitorsController(VisitorService service, ApplicationInstrumentation instrumentation)
    : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] VisitorFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.list");
        var response = await service.List(GetCongregationId(), filters, pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.fetch_by_id");
        var response = await service.GetById(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(VisitorCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, VisitorUpdateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.delete");
        var response = await service.Delete(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.search");
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }

    [HttpPost("{id}/convert")]
    public async Task<ActionResult> Convert(Guid id, MemberCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.convert");
        var response = await service.Convert(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpGet("summary")]
    public async Task<ActionResult> Summary(
        [FromQuery] VisitorFilters filters,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.visitor.summary");
        var response = await service.Summary(GetCongregationId(), filters, ct);
        return response.ToActionResult();
    }
}
