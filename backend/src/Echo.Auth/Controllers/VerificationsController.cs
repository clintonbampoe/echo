using Echo.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[Route("/api/auth/v{version:apiVersion}/verify")]
public class VerificationsController(EmailVerificationService emailVerificationService) : AuthBaseController
{
    [HttpPost("user{userId:guid}")]
    public async Task<ActionResult> SendVerificationLinkToEmail(Guid userId, CancellationToken ct = default)
    {
        var response = await emailVerificationService.SendVerificationLinkToEmail(userId, ct);
        return response.ToActionResult();
    }
}
