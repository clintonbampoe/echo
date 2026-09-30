using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Application.Transactions;

public class TransactionCategoriesController(
    TransactionCategoryService service,
    ApplicationInstrumentation instrumentation
) : BaseController
{
    [HttpGet]
    public async Task<ActionResult> List(CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.transaction_category.list");
        var response = await service.List(GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(
            instrumentation,
            "endpoint.transaction_category.fetch_by_id"
        );
        var response = await service.GetById(id, GetCongregationId(), ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(TransactionCategoryCreateDto dto, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.transaction_category.create");
        var response = await service.Create(GetCongregationId(), dto, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int id,
        TransactionCategoryUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.transaction_category.update");
        var response = await service.Update(GetCongregationId(), id, dto, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.transaction_category.delete");
        var response = await service.Delete(GetCongregationId(), id, ct);
        return response.ToActionResult();
    }

    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        using var span = StartEndpointSpan(instrumentation, "endpoint.transaction_category.search");
        var res = await service.Search(GetCongregationId(), q, ct);
        return res.ToActionResult();
    }
}
