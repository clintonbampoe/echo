using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.Organizations;

public class OrganizationMemberController(
    OrganizationMemberService service,
    ApplicationInstrumentation instrumentation
) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] OrganizationMemberFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.organization_member.list");
        var response = await service.List(GetCongregationId(), filters, pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.organization_member.fetch_by_id"
        );
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("member{id}")]
    public async Task<ActionResult> ListByMemberId(
        Guid id,
        [FromQuery] OrganizationMemberFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.organization_member.list_by_member"
        );
        var response = await service.ListByMemberId(
            GetCongregationId(),
            id,
            filters,
            pagination,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("organizations{id}")]
    public async Task<ActionResult> ListByOrganizationId(
        Guid id,
        [FromQuery] OrganizationMemberFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.organization_member.list_by_organization"
        );
        var response = await service.ListByOrganizationId(
            GetCongregationId(),
            id,
            filters,
            pagination,
            ct
        );
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(OrganizationMemberCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.organization_member.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        Guid id,
        OrganizationMemberUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.organization_member.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.organization_member.delete");
        var response = await service.Delete(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }
}
