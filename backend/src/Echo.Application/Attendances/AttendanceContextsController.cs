using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Application.Attendances;

public class AttendanceContextsController(
    AttendanceContextService service,
    ApplicationInstrumentation instrumentation
) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.attendance_context.list");
        var response = await service.List(GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.attendance_context.fetch_by_id"
        );
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(AttendanceContextCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.attendance_context.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int id,
        AttendanceContextUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.attendance_context.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.attendance_context.delete");
        var response = await service.Delete(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.attendance_context.search");
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }
}
