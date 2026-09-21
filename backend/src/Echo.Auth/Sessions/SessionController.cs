using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Sessions;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("/api/auth/v{version:ApiVersion}/[controller]")]
public class SessionController(SessionService authenticationService) : ControllerBase
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
        var response = await authenticationService.RefreshAccessToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default
    )
    {
        var response = await authenticationService.RevokeAccessToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }
}
