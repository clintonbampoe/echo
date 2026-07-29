using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers;

public class OrganizationMembersController(OrganizationMemberService service) : CoreBaseController
{
    private readonly OrganizationMemberService _service = service;

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
        var response = await _service.GetByIdAsync(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("member{id}")]
    public async Task<ActionResult> GetByMemberId(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters queryParameters,
        Guid id,
        CancellationToken ct
    )
    {
        var response = await _service.GetByMemberId(paginationParameters, queryParameters, id, ct);
        return response.ToActionResult();
    }

    [HttpGet("organizations{id}")]
    public async Task<ActionResult> GetByOrganizationId(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters queryParameters,
        Guid id,
        CancellationToken ct
    )
    {
        var response = await _service.GetByOrganizationId(
            paginationParameters,
            queryParameters,
            id,
            ct
        );
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(
        Guid id,
        OrganizationMemberUpdateDto dto,
        CancellationToken ct
    )
    {
        var response = await _service.UpdateAsync(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }
}
