using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.Events;

public class EventRegistrationsController(
    EventRegistrationService service,
    ApplicationInstrumentation instrumentation
) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event_registration.list");
        var response = await service.List(GetCongregationId(), pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.event_registration.fetch_by_id"
        );
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("event{id}")]
    public async Task<ActionResult> ListByEventId(
        Guid id,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.event_registration.list_by_event"
        );
        var response = await service.ListByEventId(GetCongregationId(), id, pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("member{id}")]
    public async Task<ActionResult> ListByMemberId(
        Guid id,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.event_registration.list_by_member"
        );
        var response = await service.ListByMemberId(GetCongregationId(), id, pagination, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(EventRegistrationCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event_registration.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        Guid id,
        EventRegistrationUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event_registration.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.event_registration.delete");
        var response = await service.Delete(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }
}
