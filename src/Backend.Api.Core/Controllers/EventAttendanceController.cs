using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
class EventAttendanceController(EventAttendanceService service) : ControllerBase
{
    private readonly EventAttendanceService _service = service;

    [HttpGet]
    public async Task<ActionResult> GetPageAsync(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var response = await _service.GetPagedAsync<EventAttendanceListResponseDto>(
            paginationParameters,
            queryParameters,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync<EventAttendanceResponseDto>(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(
        EventAttendanceCreateDto dto,
        CancellationToken ct = default
    )
    {
        var response = await _service.CreateNewRecord(dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(
        Guid id,
        EventAttendanceUpdateDto dto,
        CancellationToken ct = default
    )
    {
        var response = await _service.UpdateRecord(id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _service.DeleteRecord(id, ct);
        return response.ToActionResult();
    }
}
