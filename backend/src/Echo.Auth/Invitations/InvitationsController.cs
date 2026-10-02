using Echo.Domain.Users;
using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Invitations;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[EnableRateLimiting("auth")]
[Route("/api/v{version:apiVersion}/auth/[controller]")]
public class InvitationsController(
    InvitationService invitationService,
    AuthInstrumentation instrumentation
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateInvite(
        [FromBody] InviteRequest request,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("endpoint.invitation.create");
        span?.SetTag("congregation.id", User.GetCongregationId());
        span?.SetTag("user.id", User.GetUserId());
        span?.SetTag("user.role", User.GetUserRole());

        var response = await invitationService.CreateInvitationToken(
            User.GetCongregationId(),
            User.GetUserId(),
            request.AllowedRole,
            request.ExpiryDays,
            ct
        );

        return response.ToActionResult();
    }
}
