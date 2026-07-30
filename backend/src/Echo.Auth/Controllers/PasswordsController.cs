using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Controllers;

[EnableRateLimiting("auth")]
[AllowAnonymous]
public class PasswordController(PasswordResetService passwordResetService) : AuthBaseController
{
    [HttpPost("forgot")]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        var response = await passwordResetService.ForgotPasswordAsync(request.Email, ct);
        return response.ToActionResult();
    }

    [HttpPost("reset")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct
    )
    {
        var response = await passwordResetService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            ct
        );
        return response.ToActionResult();
    }
}
