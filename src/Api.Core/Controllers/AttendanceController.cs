using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Controllers.Base;
using Api.Core.Dtos;
using Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Controllers;

[Route("api/[controller]")]
public class AttendanceController(AttendanceService service) : BaseController
{
    private readonly AttendanceService _service = service;

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
    public async Task<ActionResult> CreateAsync(AttendanceCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(
        Guid id,
        AttendanceUpdateDto dto,
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
