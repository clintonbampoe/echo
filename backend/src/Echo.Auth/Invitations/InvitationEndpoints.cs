using Echo.Domain.Users;
using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Invitations;

public static class InvitationEndpoints
{
    public static RouteGroupBuilder MapInvitationEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        var invitations = group
            .MapGroup("/invitations")
            .WithTags("Invitations")
            .RequireRateLimiting("auth")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)));

        invitations
            .MapPost(
                "/",
                async (
                    InviteRequest request,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.create"
                    );

                    var congregationId = context.User.GetCongregationId();
                    var userId = context.User.GetUserId();
                    var role = context.User.GetUserRole();

                    span?.SetTag("congregation.id", congregationId);
                    span?.SetTag("user.id", userId);
                    span?.SetTag("user.role", role);

                    return (
                        await svc.CreateInvitationToken(
                            congregationId,
                            userId,
                            request.AllowedRole,
                            request.ExpiryDays,
                            ct
                        )
                    ).ToResult();
                }
            )
            .WithName("CreateInvitation")
            .Produces<InviteResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
