using Echo.Shared.Extensions;
using Echo.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Echo.Auth.Invitations;

[ApiController]
[Authorize(Roles = nameof(UserRole.Admin))]
[EnableRateLimiting("auth")]
[Route("/api/v{version:apiVersion}/auth/[controller]")]
public class InvitationController(InvitationService invitationService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateInvite(
        [FromBody] InviteRequest request,
        CancellationToken ct
    )
    {
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
