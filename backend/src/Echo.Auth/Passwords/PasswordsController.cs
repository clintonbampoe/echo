using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Passwords;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("auth")]
[Route("/api/auth/v{version:ApiVersion}/[controller]")]
public class PasswordsController(
    PasswordResetService passwordResetService,
    AuthInstrumentation instrumentation
) : ControllerBase
{
    [HttpPost("forgot")]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("endpoint.password.forgot");
        var response = await passwordResetService.SendForgotPasswordLinkToEmail(request.Email, ct);
        return response.ToActionResult();
    }

    [HttpPost("reset")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] PasswordResetRequest request,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("endpoint.password.reset");
        var response = await passwordResetService.ResetPassword(
            request.Token,
            request.NewPassword,
            ct
        );
        return response.ToActionResult();
    }
}
