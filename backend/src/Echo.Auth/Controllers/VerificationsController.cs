using Echo.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[Route("/api/auth/v{version:apiVersion}/[controller]")]
public class VerificationsController(EmailVerificationService emailVerificationService) : AuthBaseController
{
    [HttpPost("send-email{userId:guid}")]
    public async Task<ActionResult> SendVerificationLinkToEmail(Guid userId, CancellationToken ct = default)
    {
        var response = await emailVerificationService.SendVerificationLinkToEmail(userId, ct);
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
