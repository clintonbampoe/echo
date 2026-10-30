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
                        await svc.CreateInvitationToken(congregationId, userId, request, ct)
                    ).ToResult();
                }
            )
            .WithName("CreateInvitation")
            .WithSummary("Creates an invitation code for new users.")
            .WithDescription(
                """
                Creates an invitation code that lets people join the administrator's congregation with a specific role.

                ### What this endpoint does, and what it does not
                It **only creates the invitation**. It does not send an email or message and it creates no user account. The caller delivers the code (WhatsApp, SMS, in person), or uses `POST /auth/invitations/email` to have it emailed. The code is kept encrypted, so the invitation list can show and copy it again.

                ### Limits
                One invitation can be used by many people unless `maxUses` is set. It stops working when it **expires**, is **revoked**, or all its uses are taken (see the list, update, revoke and delete routes). Each registration adds one to its `useCount`.

                ### Request body
                - `allowedRole` — **required.** The role new users receive: `Admin`, `Accountant`, `Clerk` or `Member`.
                - `expiryDays` — optional, 1 to 365. Defaults to 30. Do not send it together with `neverExpires`.
                - `neverExpires` — optional. `true` for an invitation with no expiry date; `expiresAt` is then `null`.
                - `maxUses` — optional, 1 to 10000: how many people may join with it. Leave it out for no limit, in which case `maxUses` is `null`.
                - `label` — optional, up to 100 characters. A name to tell invitations apart in the list, such as "Youth group link".

                ### On success
                `200 OK` with an `InviteResponseDto`:
                - `id` — use it to update, revoke or delete the invitation.
                - `token` — the invitation code, grouped like `K7QX-92MD-PW4T`: 12 characters from an alphabet without I, L, O or U. People can type it in any case, with or without dashes or spaces.
                - `allowedRole`, `expiresAt` (UTC, authoritative; `null` if it never expires), `maxUses`, `label`.

                ### Who can call this
                `Admin` only. The invitation always belongs to the caller's congregation, taken from the bearer token.

                ### Failure modes
                - `400 BAD_REQUEST` — both `expiryDays` and `neverExpires` were sent.
                - `400 VALIDATION_ERROR` — invalid `allowedRole`, `expiryDays` outside 1–365, `maxUses` outside 1–10000, or a label over 100 characters.
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
            .MapPost(
                "/email",
                async (
                    [FromBody] InviteByEmailRequest request,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.email"
                    );
                    return (
                        await svc.CreateAndEmail(
                            context.User.GetCongregationId(),
                            context.User.GetUserId(),
                            request,
                            ct
                        )
                    ).ToResult();
                }
            )
            .WithName("InviteByEmail")
            .WithSummary("Creates a single-use invitation and emails it.")
            .WithDescription(
                """
                Creates an invitation for one person and sends it to their email address. The email has a join link (`/join/<code>`) and the code itself.

                ### What it creates
                A normal invitation that can be used **once** (`maxUses` is 1). It appears in the invitation list like any other, labelled with the email address unless you give a `label`.

                ### Request body
                - `emailAddress` — **required.** Where to send the invitation. Up to 255 characters.
                - `allowedRole` — **required.** The role the person receives: `Admin`, `Accountant`, `Clerk` or `Member`.
                - `expiryDays` — optional, 1 to 365. Defaults to 30. Do not send it together with `neverExpires`.
                - `neverExpires` — optional. `true` for no expiry date.
                - `label` — optional, up to 100 characters. Defaults to the email address.

                ### On success
                `200 OK` with an `InviteResponseDto`: `id`, `token` (the code), `allowedRole`, `expiresAt`, `maxUses` and `label`, plus `emailedTo`.

                `emailedTo` is the address the email went to. If it is `null` the invitation **was created but the email could not be sent**: copy the `token` and deliver it another way, or revoke the invitation and try again.

                ### Who can call this
                `Admin` only; the invitation belongs to the caller's congregation.

                ### Failure modes
                - `400 BAD_REQUEST` — both `expiryDays` and `neverExpires` were sent.
                - `400 VALIDATION_ERROR` — `emailAddress` missing or not an email address, invalid `allowedRole`, `expiryDays` outside 1–365, or a label over 100 characters.
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
                "/summary",
                async (InvitationService svc, HttpContext context, CancellationToken ct) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.summary"
                    );
                    return (await svc.GetSummary(context.User.GetCongregationId(), ct)).ToResult();
                }
            )
            .WithName("GetInvitationSummary")
            .WithSummary("Returns the counts shown above the invitation list.")
            .WithDescription(
                """
                Returns counts for the whole congregation, not just the page of invitations on screen, so they stay right past the first page. The answer is cached for 2 minutes, so a new invitation can take up to that long to show in the counts.

                ### Response fields
                - `active` — invitations that can be used right now: not revoked, not expired, and with uses left.
                - `peopleJoined` — people who registered with any of the congregation's invitations, whatever their status now.
                - `expiringSoon` — active invitations that expire within the next 7 days. Invitations that never expire are not counted.

                This endpoint takes no parameters.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `429`.
                """
            )
            .Produces<InvitationSummaryDto>(StatusCodes.Status200OK)
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
                Returns a cursor-paginated list of the congregation's invitations, newest first, then by id. Each row carries its `code`, so the list can copy it.

                ### Query parameters
                - `status` — optional: `Active`, `Expired`, `Revoked` or `Exhausted`. Leave it out to list them all.
                - `cursor`, `pageSize` — as on the other list endpoints.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Response items
                `id`, `label`, `allowedRole`, `createdByName`, `createdAt`, `expiresAt` (`null` if it never expires), `status`, `useCount` (how many people registered with it), `maxUses` (`null` if unlimited) and `code`.

                `code` is the invitation code, grouped like `K7QX-92MD-PW4T`. It is `null` for invitations made before codes were kept: those were shown only once, when created.

                `status` is `Revoked` if revoked, otherwise `Expired` once `expiresAt` has passed, otherwise `Exhausted` once `useCount` reaches `maxUses`, otherwise `Active`.

                For counts that do not depend on the page, use `GET /auth/invitations/summary`.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `429`.
                """
            )
            .Produces<PagedResponse<InvitationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        invitations
            .MapGet(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    InvitationService svc,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = instrumentation.ActivitySource.StartActivity(
                        "endpoint.invitation.get_by_id"
                    );
                    return (await svc.GetById(context.User.GetCongregationId(), id, ct)).ToResult();
                }
            )
            .WithName("GetInvitation")
            .WithSummary("Returns one invitation.")
            .WithDescription(
                """
                Returns a single invitation of the administrator's congregation, in the same shape as a row of `GET /auth/invitations`: `id`, `label`, `allowedRole`, `createdByName`, `createdAt`, `expiresAt`, `status`, `useCount`, `maxUses` and `code`.

                ### Path parameters
                - `id` — the invitation's id, as returned by the create and list endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED`, `403 FORBIDDEN` (not an `Admin`), `429`.
                - `404 NOT_FOUND` — no such invitation in this congregation, or it was deleted.
                """
            )
            .Produces<InvitationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
            .WithSummary("Renames an invitation or changes when it expires or how many can join.")
            .WithDescription(
                """
                Changes an invitation's `label`, its expiry and/or its use limit. Only the fields sent change. The role cannot be changed: create a new invitation instead.

                ### Path parameters
                - `id` — the invitation's id, as returned by the create and list endpoints.

                ### Request body
                - `label` — up to 100 characters; an empty string clears it.
                - `expiryDays` — 1 to 365; the invitation now expires that many days **from now**, which also reactivates an expired one. Do not send it with `neverExpires`.
                - `neverExpires` — `true` to remove the expiry date. To give it an expiry date again, send `expiryDays`.
                - `maxUses` — 1 to 10000: the new limit, counting the people who already joined, which reactivates an invitation that was used up. It cannot be lower than `useCount`. A limit cannot be removed: create a new unlimited invitation instead.

                A **revoked** invitation cannot be extended.

                ### Failure modes
                - `400 BAD_REQUEST` — extending a revoked invitation, sending both `expiryDays` and `neverExpires`, or a `maxUses` below `useCount`; `400 VALIDATION_ERROR` — out-of-range values.
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
                `congregationName`, `city`, `region`, `allowedRole` and `expiresAt` (`null` if the invitation never expires).

                ### Failure modes
                - `404 NOT_FOUND` — the code is wrong, expired, revoked, used up or deleted. All of these give the same response on purpose, so the endpoint reveals nothing about which codes exist.
                - `429` — rate limited per IP like the other auth routes.
                """
            )
            .Produces<InvitationLookupDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
