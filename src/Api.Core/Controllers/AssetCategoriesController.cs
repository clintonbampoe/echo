using Api.Core.Controllers.Base;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Core.Controllers;

[Route("api/[controller]")]
public class AssetCategoriesController(AssetCategoryService service) : BaseController
{
    private readonly AssetCategoryService _service = service;

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
    public async Task<ActionResult> CreateAsync(AssetCategoryCreateDto dto, CancellationToken ct)
    {
        var response = await _service.CreateAsync(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(
        int id,
        AssetCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        var response = await _service.UpdateAsync(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);
        return response.ToActionResult();
    }
}
