using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Passwords;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("/api/auth/v{version:ApiVersion}/[controller]")]
public class PasswordController(PasswordResetService passwordResetService) : ControllerBase
{
    [HttpPost("forgot")]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        var response = await passwordResetService.SendForgotPasswordLinkToEmail(request.Email, ct);
        return response.ToActionResult();
    }

    [HttpPost("reset")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] PasswordResetRequest request,
        CancellationToken ct
    )
    {
        var response = await passwordResetService.ResetPassword(
            request.Token,
            request.NewPassword,
            ct
        );
        return response.ToActionResult();
    }
}
