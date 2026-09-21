using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application.Organizations;

public class OrganizationMemberController(OrganizationMemberService service) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] OrganizationMemberFilters filters,
        [FromQuery] PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var response = await service.List(GetCongregationId(), filters, pagination, ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
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
