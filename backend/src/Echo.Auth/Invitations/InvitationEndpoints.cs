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
            .WithSummary("Creates an invitation token for a new user.")
            .WithDescription(
                """
                Mints an invitation token that allows a new user to join the authenticated administrator's congregation with a specific role.

                ### What this endpoint does — and what it does not
                This endpoint **only creates the token**. It does not send an email, it does not create a user account, and it does not notify the invited person in any way.

                The response contains the raw token. **The caller is responsible for delivering it** — emailing it, generating a signup link, pasting it into a chat, printing it. The server does not do this for you.

                Until the token is redeemed via `POST /auth/registrations/member`, the invitation is inert. No user record exists yet, and nothing about the invited email address is stored.

                ### Request body
                - `allowedRole` — **required.** The `UserRole` the invited user will receive on registration. One of `Admin`, `Accountant`, `Clerk`, `Member`.
                - `expiryDays` — optional. The requested lifetime in days. `null` uses the server default.

                **Server cap: 30 days.** The DTO accepts values up to `365`, but the server silently clamps anything above 30 down to 30. Requesting `expiryDays: 365` does not produce an error — it produces a token that expires in 30 days. Do not surface a "valid for 365 days" message in the UI based on the request value; use the `expiresAt` from the response instead.

                ### On success
                Returns `200 OK` with an `InviteResponseDto`:

                - `token` — the invitation token. Deliver this to the invited user out of band. Do not log it, do not put it in a URL the user might share back.
                - `allowedRole` — echoes back the role that was actually applied.
                - `expiresAt` — UTC timestamp when the token becomes invalid. Use this to build any UI copy like "expires on …". This is the authoritative value; ignore whatever the client requested.

                ### Who can call this
                An authenticated user with the `Admin` role in their congregation. Any other role returns `403`.

                The invitation is always scoped to the caller's congregation — derived from the bearer token, never from the request body. An admin cannot invite into another tenant, and there is no parameter that would let them try.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed (missing `allowedRole`, or an invalid value).
                - `400 VALIDATION_ERROR` — `allowedRole` is not a valid `UserRole` enum value, or `expiryDays` is outside the accepted range (1–365). Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `403 FORBIDDEN` — authenticated, but the caller does not have the `Admin` role.

                ### Authentication
                Bearer token required, `Admin` role required. See the **Invitations** tag description for the full flow including redemption.

                ### Rate limiting
                This endpoint is rate-limited per the `auth` policy. Excessive requests return `429 Too Many Requests`.
                """
            )
            .Produces<InviteResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
