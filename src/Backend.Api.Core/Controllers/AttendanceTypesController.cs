using Backend.Api.Core.Controllers.Base;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;

[Route("api/[controller]")]
public class AttendanceTypesController(AttendanceTypeService service) : BaseController
{
    private readonly AttendanceTypeService _service = service;

    [HttpGet]
    public async Task<ActionResult> GetAllAsync(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(AttendanceTypeCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(
        int id,
        AttendanceTypeUpdateDto dto,
        CancellationToken ct
    )
    {
        var response = await _service.UpdateAsync(id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);
        return response.ToActionResult();
    }
}
