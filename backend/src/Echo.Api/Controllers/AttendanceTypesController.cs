using Echo.Application.HttpResults;
using Echo.Core.Services;
using Echo.Core.Dtos;
using Microsoft.AspNetCore.Mvc;
using Echo.Api.Extensions;

namespace Echo.Api.Controllers;

public class AttendanceTypesController(AttendanceTypeService service) : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await service.List(GetCongregationId(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await service.GetById(id, GetCongregationId(), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AttendanceTypeCreateDto dto, CancellationToken ct)
    {
        var result = await service.Create(GetCongregationId(), dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AttendanceTypeUpdateDto dto, CancellationToken ct)
    {
        var result = await service.Update(GetCongregationId(), id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await service.Delete(GetCongregationId(), id, ct);
        return result.ToActionResult();
    }
}
