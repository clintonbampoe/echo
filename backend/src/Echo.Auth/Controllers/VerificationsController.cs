using Echo.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Controllers;

[AllowAnonymous]
[EnableRateLimiting("auth")]
public class VerificationsController(EmailVerificationService emailVerificationService)
    : AuthBaseController
{
    [HttpPost("account{email}")]
    public async Task<ActionResult> SendVerificationLinkToEmail(
        string email,
        CancellationToken ct = default
    )
    {
        var response = await emailVerificationService.SendVerificationLinkToEmail(email, ct);
        return response.ToActionResult();
    }

    /// <summary>
    /// Accepts email verification token as payload and verifies the user attached to that token
    /// </summary>
    [HttpPost("verify-email{token}")]
    public async Task<ActionResult> VerifyUserEmail(string token, CancellationToken ct = default)
    {
        var response = await emailVerificationService.VerifyEmail(token, ct);
        return response.ToActionResult();
    }
}
