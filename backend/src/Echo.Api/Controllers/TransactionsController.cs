using global::Echo.Application.HttpResults;
using global::Echo.Application.Pagination;
using global::Echo.Core.Services;
using global::Echo.Core.Dtos;
using Microsoft.AspNetCore.Mvc;
using Echo.Api.Extensions;

namespace Echo.Api.Controllers;

public class TransactionsController(TransactionService service) : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] TransactionFilters filters, [FromQuery] PaginationRequest pagination, CancellationToken ct)
    {
        var result = await service.List(GetCongregationId(), filters, pagination, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetById(id, GetCongregationId(), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionCreateDto dto, CancellationToken ct)
    {
        var result = await service.Create(GetCongregationId(), dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TransactionUpdateDto dto, CancellationToken ct)
    {
        var result = await service.Update(GetCongregationId(), id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await service.Delete(GetCongregationId(), id, ct);
        return result.ToActionResult();
    }
}
