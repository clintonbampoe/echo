using Echo.Application.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Attendances;

public class AttendanceController(AttendanceService service) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] AttendanceFilters filters,
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
    public async Task<ActionResult> Create(AttendanceCreateDto dto, CancellationToken ct)
    {
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, AttendanceUpdateDto dto, CancellationToken ct)
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
