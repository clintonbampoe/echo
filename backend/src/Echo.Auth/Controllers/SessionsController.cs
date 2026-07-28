using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[AllowAnonymous]
public class SessionsController(AuthenticationService authenticationService) : AuthBaseController
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        var response = await authenticationService.LoginAsync(request.Email, request.Password, ct);
        return response.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct = default)
    {
        var response = await authenticationService.RefreshAsync(request.RefreshToken, ct);
        return response.ToActionResult();
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        var response = await authenticationService.LogoutAsync(request.RefreshToken, ct);
        return response.ToActionResult();
    }

}
