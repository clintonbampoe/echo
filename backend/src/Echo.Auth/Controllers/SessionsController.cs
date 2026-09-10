using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Controllers;

[AllowAnonymous]
[EnableRateLimiting("auth")]
public class SessionsController(AuthenticationService authenticationService) : AuthBaseController
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default
    )
    {
        var response = await authenticationService.Login(request.Email, request.Password, ct);
        return response.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default
    )
    {
        var response = await authenticationService.RefreshAuthToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default
    )
    {
        var response = await authenticationService.RevokeAuthToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }
}
