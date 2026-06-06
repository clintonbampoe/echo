using Backend.Api.Core.Dtos;
using Backend.Api.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Core.Controllers;
[Route("api/[controller]")]
[ApiController]
public class MembersController : ControllerBase
{
    private readonly MemberService _service;
    public MembersController(
        MemberService service
    )
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetPageAsync(
        CancellationToken ct,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] QueryParameters? queryParameters
    )
    {
        var response = await _service.GetPagedAsync<MemberListResponseDto>(paginationParameters, queryParameters, ct);
        return response.ToActionResult();
    }


    [HttpPost]
    public async Task<ActionResult> Update(
        MemberCreateDto member,
        CancellationToken ct
    )
    {
        var response = await _service.CreateNewRecord(member, ct);
        return response.ToActionResult();
    }
}
