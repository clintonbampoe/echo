using Echo.Core.Dtos;
using Echo.Core.Services;
using Echo.Shared.Controllers;
using Echo.Shared.Pagination;
using Echo.Shared.Query;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers;

[Route("api/[controller]")]
public class OrganizationMembersController(OrganizationMemberService service) : BaseController
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
        var response = await _service.GetByIdAsync(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(
        OrganizationMemberCreateDto dto,
        CancellationToken ct
    )
    {
        var response = await _service.CreateAsync(GetCongregationId(), dto, ct);
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
        var response = await _service.DeleteAsync(id, ct);
        return response.ToActionResult();
    }
}
