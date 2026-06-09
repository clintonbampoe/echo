using Backend.Api.Core.Common.Pagination;
using Backend.Api.Core.Common.Query;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MembersController(MemberService service) : ControllerBase
{
    private readonly MemberService _service = service;

    [HttpGet]
    public async Task<ActionResult> GetPageAsync(
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters,
        CancellationToken ct
    )
    {
        var response = await _service.GetPagedAsync<MemberListResponseDto>(
            paginationParameters,
            queryParameters,
            ct
        );
        return response.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync<MemberResponseDto>(id, ct);
        return response.ToActionResult();
    }

    [HttpPost]
    public async Task<ActionResult> Create(MemberCreateDto member, CancellationToken ct)
    {
        var response = await _service.CreateNewRecord(member, ct);
        return response.ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        Guid id,
        MemberUpdateDto memberData,
        CancellationToken ct
    )
    {
        var response = await _service.UpdateRecord(id, memberData, ct);
        return response.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var response = await _service.DeleteRecord(id, ct);
        return response.ToActionResult();
    }
}
