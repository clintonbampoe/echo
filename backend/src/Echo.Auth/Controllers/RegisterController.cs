using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Controllers;

[EnableRateLimiting("auth")]
[AllowAnonymous]
public class RegisterController(RegistrationService service) : AuthBaseController
{
    private readonly RegistrationService _service = service;

    [HttpPost("congregation")]
    public async Task<ActionResult> RegisterCongregation(
        [FromBody] RegisterCongregationRequest request,
        CancellationToken ct
    )
    {
        var response = await _service.RegisterCongregation(
            request.CongregationDto,
            request.UserDto,
            ct
        );
        return response.ToActionResult();
    }

    [HttpPost("member")]
    [AllowAnonymous]
    public async Task<ActionResult> RegisterMember(
        [FromBody] RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        var response = await _service.RegisterMemberAsync(request, ct);
        return response.ToActionResult();
    }
}
