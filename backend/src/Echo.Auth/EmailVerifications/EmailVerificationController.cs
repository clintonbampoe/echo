using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.EmailVerifications;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("/api/auth/v{version:ApiVersion}/[controller]")]
public class EmailVerificationController(EmailVerificationService emailVerificationService)
    : ControllerBase
{
    [HttpPost("account")]
    public async Task<ActionResult> SendUserVerificationLink(
        [FromBody] EmailVerificationLinkRequest request,
        CancellationToken ct = default
    )
    {
        var response = await emailVerificationService.SendVerificationLinkToEmail(
            request.Email,
            ct
        );
        return response.ToActionResult();
    }

    /// <summary>
    /// Accepts email verification token as payload and verifies the user attached to that token
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyUser(
        [FromQuery] string token,
        CancellationToken ct = default
    )
    {
        var response = await emailVerificationService.VerifyEmail(token, ct);
        return response.ToActionResult();
    }
}
