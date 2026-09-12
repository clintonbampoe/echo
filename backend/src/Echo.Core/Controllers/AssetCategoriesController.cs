using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Core.Controllers;

public class AssetCategoriesController(AssetCategoryService service) : CoreBaseController
{
    [HttpGet]
    public async Task<ActionResult> List(CancellationToken ct)
    {
        var res = await service.List(GetCongregationId(), ct);
        return res.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        var res = await service.GetById(id, GetCongregationId(), ct);
        return res.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(AssetCategoryCreateDto dto, CancellationToken ct)
    {
        var res = await service.Create(GetCongregationId(), dto, ct);
        return res.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, AssetCategoryUpdateDto dto, CancellationToken ct)
    {
        var res = await service.Update(GetCongregationId(), id, dto, ct);
        return res.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var res = await service.Delete(GetCongregationId(), id, ct);
        return res.ToActionResult();
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }
}
