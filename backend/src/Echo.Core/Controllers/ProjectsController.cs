using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers;

public class ProjectsController(ProjectService service) : CoreBaseController
{
    private readonly ProjectService _service = service;

    [HttpGet("summary")]
    public async Task<ActionResult> GetSummaryAsync(CancellationToken ct)
    {
        var response = await _service.GetSummaryAsync(GetCongregationId(), ct);
        return response.ToActionResult();
    }

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

    [HttpPost]
    public async Task<ActionResult> CreateAsync(ProjectCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, ProjectUpdateDto dto, CancellationToken ct)
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
