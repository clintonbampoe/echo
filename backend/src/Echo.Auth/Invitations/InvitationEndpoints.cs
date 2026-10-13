using Echo.Domain.Users;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Auth.Invitations;

public static class InvitationEndpoints
{
    public static RouteGroupBuilder MapInvitationEndpoints(
        this RouteGroupBuilder group,
        AuthInstrumentation instrumentation
    )
    {
        // Managing invitations is for admins only.
        var invitations = group
            .MapGroup("/invitations")
            .WithTags("Invitations")
            .RequireRateLimiting("auth")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)));

        invitations
            .MapPost(
                "/",
                async (
                    [FromBody] InviteRequest request,
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

                    span?.SetTag("congregation.id", congregationId);
                    span?.SetTag("user.id", userId);

                    return (
                        await svc.CreateInvitationToken(
                            congregationId,
                            userId,
                            request.AllowedRole,
                            request.ExpiryDays,
                            request.Label,
                            ct
                        )
                    ).ToResult();
                }
            )
            .WithName("CreateInvitation")
            .WithSummary("Creates an invitation code for new users.")
            .WithDescription(
                """
                Creates an invitation code that lets people join the administrator's congregation with a specific role.

                ### What this endpoint does, and what it does not
                It **only creates the invitation**. It does not send an email or message and it creates no user account. The response contains the code **once**; the server keeps only a hash, so it cannot be shown again. The caller delivers it (WhatsApp, SMS, email, in person).

                ### Reusable until it expires
                One invitation can be used by many people. It stops working when it **expires** or is **revoked** (see the list, update, revoke and delete routes). Each registration adds one to its `useCount`.

                ### Request body
                - `allowedRole` — **required.** The role new users receive: `Admin`, `Accountant`, `Clerk` or `Member`.
                - `expiryDays` — optional, 1 to 30. Defaults to 30.
                - `label` — optional, up to 100 characters. A name to tell invitations apart in the list, such as "Youth group link".

                ### On success
                `200 OK` with an `InviteResponseDto`:
                - `id` — use it to update, revoke or delete the invitation.
                - `token` — the invitation code, grouped like `K7QX-92MD-PW4T`: 12 characters from an alphabet without I, L, O or U. People can type it in any case, with or without dashes or spaces.
                - `allowedRole`, `expiresAt` (UTC, authoritative), `label`.

                ### Who can call this
                `Admin` only. The invitation always belongs to the caller's congregation, taken from the bearer token.

                ### Failure modes
                - `400 VALIDATION_ERROR` — invalid `allowedRole`, `expiryDays` outside 1–30, or a label over 100 characters.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `403 FORBIDDEN` — the caller is not an `Admin`.
                - `429` — too many requests (the `auth` rate limit).
                """
            )
            .Produces<InviteResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        invitations
            .MapGet(
                "/",
                async (
                    [AsParameters] InvitationFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.list"
                    );
                    return (
                        await svc.List(context.User.GetCongregationId(), filters, pagination, ct)
                    ).ToResult();
                }
            )
            .WithName("ListInvitations")
            .WithSummary("Lists the congregation's invitations, newest first.")
            .WithDescription(
                """
                Returns a cursor-paginated list of the congregation's invitations, newest first. The invitation code is never returned: only its hash is stored.

                ### Query parameters
                - `status` — optional: `Active`, `Expired` or `Revoked`.
                - `cursor`, `pageSize` — as on the other list endpoints.

                ### Response items
                `id`, `label`, `allowedRole`, `createdByName`, `createdAt`, `expiresAt`, `status` and `useCount` (how many people registered with it). `status` is `Revoked` if revoked, otherwise `Expired` once `expiresAt` has passed, otherwise `Active`.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `429`.
                """
            )
            .Produces<PagedResponse<InvitationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        invitations
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] InvitationUpdateDto dto,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.update"
                    );
                    return (
                        await svc.Update(context.User.GetCongregationId(), id, dto, ct)
                    ).ToResult();
                }
            )
            .WithName("UpdateInvitation")
            .WithSummary("Renames an invitation or changes when it expires.")
            .WithDescription(
                """
                Changes an invitation's `label` and/or its expiry. Only the fields sent change. The role cannot be changed: create a new invitation instead.

                - `label` — up to 100 characters; an empty string clears it.
                - `expiryDays` — 1 to 30; the invitation now expires that many days **from now**, which also reactivates an expired one. A **revoked** invitation cannot be extended.

                ### Failure modes
                - `400 BAD_REQUEST` — extending a revoked invitation; `400 VALIDATION_ERROR` — out-of-range values.
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `404 NOT_FOUND`, `429`.
                """
            )
            .Produces<InvitationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        invitations
            .MapPost(
                "/{id:guid}/revoke",
                async (
                    [FromRoute] Guid id,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.revoke"
                    );
                    return (await svc.Revoke(context.User.GetCongregationId(), id, ct)).ToResult();
                }
            )
            .WithName("RevokeInvitation")
            .WithSummary("Stops an invitation from working.")
            .WithDescription(
                """
                Revokes the invitation: nobody can register with it from now on, and the join screen no longer recognises it. It stays in the list with status `Revoked`, with its `useCount`. People who already registered keep their accounts. Revoking again changes nothing.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `404 NOT_FOUND`, `429`.
                """
            )
            .Produces<InvitationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        invitations
            .MapDelete(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.delete"
                    );
                    return (await svc.Delete(context.User.GetCongregationId(), id, ct)).ToResult();
                }
            )
            .WithName("DeleteInvitation")
            .WithSummary("Removes an invitation.")
            .WithDescription(
                """
                Deletes the invitation: it disappears from the list and stops working. Unlike revoking, it is no longer shown at all. People who already registered keep their accounts.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `404 NOT_FOUND`, `429`.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // The join screen reads this before the person has an account, so it is anonymous.
        var join = group.MapGroup("/invitations").WithTags("Invitations").RequireRateLimiting("auth");

        join.MapGet(
                "/{code}",
                async ([FromRoute] string code, InvitationService svc, CancellationToken ct) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.lookup"
                    );
                    return (await svc.Lookup(code, ct)).ToResult();
                }
            )
            .AllowAnonymous()
            .WithName("LookupInvitation")
            .WithSummary("Shows which congregation and role an invitation code is for.")
            .WithDescription(
                """
                Lets the join screen confirm the congregation and role before the person creates an account. **No sign-in needed.**

                The code can be in any case, with or without dashes or spaces.

                ### On success
                `congregationName`, `city`, `region`, `allowedRole` and `expiresAt`.

                ### Failure modes
                - `404 NOT_FOUND` — the code is wrong, expired, revoked or deleted. All four give the same response on purpose, so the endpoint reveals nothing about which codes exist.
                - `429` — rate limited per IP like the other auth routes.
                """
            )
            .Produces<InvitationLookupDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
