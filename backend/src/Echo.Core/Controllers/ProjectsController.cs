using Echo.Application.Pagination;
using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Core.Controllers;

public class ProjectsController(ProjectService service) : CoreBaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] ProjectFilters filters,
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

    [HttpPost]
    public async Task<ActionResult> Create(ProjectCreateDto dto, CancellationToken ct)
    {
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, ProjectUpdateDto dto, CancellationToken ct)
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

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }
}
