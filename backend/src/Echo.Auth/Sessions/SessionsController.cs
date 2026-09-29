using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Sessions;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("/api/auth/v{version:ApiVersion}/[controller]")]
public class SessionsController(SessionService sessionService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct = default
    )
    {
        var response = await sessionService.Login(request.Email, request.Password, ct);
        return response.ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> LogoutOfAllSessions(
        [FromBody] LogoutAllSessionsRequest request,
        CancellationToken ct = default
    )
    {
        var response = await sessionService.LogoutOfAllSessions(request.Email, ct);
        return response.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default
    )
    {
        var response = await sessionService.RefreshAccessToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> LogoutOfCurrentSession(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct = default
    )
    {
        var response = await sessionService.RevokeAccessToken(request.RefreshToken, ct);
        return response.ToActionResult();
    }
}
