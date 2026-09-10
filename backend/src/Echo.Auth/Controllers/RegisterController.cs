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
    [HttpPost("congregation")]
    public async Task<ActionResult> RegisterCongregation(
        [FromBody] RegisterCongregationRequest request,
        CancellationToken ct
    )
    {
        var res = await service.RegisterCongregation(request.CongregationDto, request.UserDto, ct);
        return res.ToActionResult();
    }

    [HttpPost("member")]
    public async Task<ActionResult> RegisterMember(
        [FromBody] RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        var res = await service.RegisterUser(request, ct);
        return res.ToActionResult();
    }
}
