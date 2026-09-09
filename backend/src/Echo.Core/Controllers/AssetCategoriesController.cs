using Echo.Core.Controllers.Base;
using Echo.Core.Dtos;
using Echo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers;

public class AssetCategoriesController(AssetCategoryService service) : CoreBaseController
{
    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var response = await service.GetAll(GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(AssetCategoryCreateDto dto, CancellationToken ct)
    {
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int id,
        AssetCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var response = await service.Delete(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }
}
