using Echo.Application.Extensions;
using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Echo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[Route("/api/v{version:apiVersion}/auth/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class InvitationsController(InvitationService invitationService) : AuthBaseController
{
    [HttpPost]
    public async Task<ActionResult> CreateInvite([FromBody] InviteRequest request, CancellationToken ct)
    {
        var response = await invitationService.CreateInvitationAsync(
            User.GetCongregationId(), User.GetUserId(), request.AllowedRole, request.ExpiryDays, ct);

        return response.ToActionResult();
    }
}
