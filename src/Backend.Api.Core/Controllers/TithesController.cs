using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TitheController(TitheService service) : ControllerBase
{
    private readonly TitheService _service = service;

    [HttpGet("summary")]
    public async Task<ActionResult> GetSummary(
        Guid congregationId,
        [FromQuery] int year,
        CancellationToken ct
    )
    {
        var response = await _service.GetSummaryAsync(congregationId, year, ct);
        return response.ToActionResult();
    }

    [HttpGet]
    public async Task<ActionResult> GetPageAsync(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var response = await _service.GetPagedAsync<TitheListResponseDto>(
            paginationParameters,
            queryParameters,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync<TitheResponseDto>(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(TitheCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateNewRecord(dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, TitheUpdateDto dto, CancellationToken ct)
    {
        var response = await _service.UpdateRecord(id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.DeleteRecord(id, ct);
        return response.ToActionResult();
    }
}
