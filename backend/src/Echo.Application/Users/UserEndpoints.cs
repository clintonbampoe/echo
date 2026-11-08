using System.ComponentModel.DataAnnotations;
using Echo.Application.Caching;
using Echo.Domain.Users;
using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Application.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .CacheOutput(ReadCache.Policy);

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] PaginationRequest pagination,
                    [AsParameters] UserFilter filter,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        pagination,
                        filter,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListUsers")
            .WithSummary("Returns a paginated list of users.")
            .WithDescription(
                """
                Returns a cursor-paginated list of platform accounts scoped to the authenticated user's congregation. Only users that belong to the same congregation as the caller are returned.

                ### What is in the list
                Each entry is a `UserResponseDto` — the platform account, not the Member record. See the **Users** tag description for the distinction. If you need people records, use `/members` instead.

                ### Filters
                - `role` — only people with this role (`Admin`, `Accountant`, `Clerk`, `Member`).
                - `status` — `Active` (can sign in), `Disabled` (an admin turned the account off) or `Unverified` (email not verified yet).

                - `q` — text to find in the name or email, ignoring case. From three characters it matches anywhere in the name or email; shorter text matches only the start.

                All are optional and combine. Leave them out for everyone.

                ### Ordering
                Users are ordered by name, then by ID as a tiebreaker. The order is stable across pages.

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### Authentication
                Bearer token required. The list is automatically scoped to the caller's congregation — there is no parameter to override this.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Not rate-limited beyond the platform default.
                """
            )
            .Produces<PagedResponse<UserResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetUserById")
            .WithSummary("Returns a single user by ID.")
            .WithDescription(
                """
                Returns the full account record for the given user ID, scoped to the caller's congregation.

                ### On success
                Returns `200 OK` with a `UserResponseDto`. Includes `verifiedAt` — null means the account exists but has not completed email verification and cannot sign in.

                ### Path parameters
                - `id` — the user's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has been soft-deleted. Both cases return the same code — the API does not reveal whether a record exists elsewhere.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] UserCreateDto dto,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.create"
                    );
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateUser")
            .WithSummary("Creates a new user.")
            .WithDescription(
                """
                Creates a new platform account scoped to the caller's congregation.

                ### Request body
                - `firstName`, `lastName` — required.
                - `emailAddress` — required. Must be unique across the platform, not just within the congregation.
                - `password` — required, minimum 8 characters.
                - `otherNames` — optional.
                - `role` — optional `UserRole`. One of `Admin`, `Accountant`, `Clerk`, `Member`. Defaults to `Member` if omitted.

                ### On success
                Returns `201 Created` with the full `UserResponseDto` and a `Location` header pointing to `GET /users/{id}`.

                **The new account is unverified.** `verifiedAt` is null. The user cannot sign in until they complete email verification. The server may send a verification email automatically; if not, the client should trigger `POST /auth/verifications/account` with the new email.

                ### Side effects
                - A new user record is created in the caller's congregation.
                - A verification email may be sent to the supplied address.
                - No session is created for the new user. They will log in separately via `/auth/sessions/login` after verifying.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `409 CONFLICT` — a user with the supplied email already exists. In this build the service returns this as `400 BAD_REQUEST` with the message "Email already exists or is invalid." — check the response body's `detail` field to distinguish from a genuine malformed-body failure.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] UserUpdateDto dto,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.update"
                    );
                    var result = await service.Update(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        id,
                        dto,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("UpdateUser")
            .WithSummary("Updates an existing user.")
            .WithDescription(
                """
                Updates the supplied fields on an existing user record. This is a partial update — only fields present in the request body are changed. Omitted fields retain their current values.

                ### Request body
                - `emailAddress` — optional. If supplied, must remain unique across the platform.
                - `password` — optional. If supplied, must meet the same policy as create (minimum 8 characters).
                - `role` — optional. Changing the role takes effect immediately on the user's next request.

                **Do not** send fields you do not intend to change — sending `"role": null` will clear the role.

                ### On success
                Returns `200 OK` with the full updated `UserResponseDto`.

                ### Side effects
                - If the password changed, existing sessions are **not** revoked by this endpoint. The user remains signed in on other devices until their access tokens expire and their refresh tokens are next used. If you need to force a sign-out, call `POST /auth/sessions/logout` with the user's email afterward.
                - The role is read from the access token, so a role change shows up when the person's access token is next refreshed (within a few minutes) or when they sign in again.
                - If the role changed, the person gets an email saying their role went up or down, who changed it and when. A failure to queue the email never undoes the change.
                - If the email changed, the account's verified state is **preserved as-is** — this endpoint does not reset verification. If your policy requires re-verification on email change, that must be handled separately.

                ### Path parameters
                - `id` — the user's id, as returned by the list and create endpoints.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has been soft-deleted.
                - `409 CANNOT_MODIFY_SELF` — you tried to change your own role. Another admin has to do it.
                - `409 LAST_ADMIN` — the change would leave the congregation with no admin who can sign in.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.delete"
                    );
                    var result = await service.Delete(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        id,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("DeleteUser")
            .WithSummary("Soft deletes a user.")
            .WithDescription(
                """
                Soft-deletes the user record. The row is retained in the database but the account can no longer authenticate and no longer appears in list, search, or lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The user is marked as deleted.
                - The person gets an email saying their account was removed.
                - **Refresh tokens are revoked**, so the person is signed out of every device. An access token already issued keeps working until it expires (up to ~15 minutes).
                - Records that reference this user — as an event organizer, project manager, etc. — are not affected. The references remain, though lookups of the underlying user will now return `404`.

                ### Reversibility
                Soft delete is not exposed as reversible through the API. Restoring a deleted user requires database access.

                ### Path parameters
                - `id` — the user's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has already been soft-deleted.
                - `409 CANNOT_MODIFY_SELF` — you tried to remove your own account.
                - `409 LAST_ADMIN` — this is the only admin who can sign in.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet(
                "/summary",
                async (UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.summary"
                    );
                    var result = await service.Summary(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("GetUserSummary")
            .WithSummary("Returns how many people hold each role.")
            .WithDescription(
                """
                Returns the head counts the Team page shows: the total, how many accounts are disabled, and one entry per role with its count and how many of those are disabled.

                ### On success
                Returns `200 OK` with a `UserSummaryDto`. `roles` always has one entry for every role, in role order, even a role nobody holds, so a client can draw a card for each.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `403 FORBIDDEN` — the caller is not an admin.
                """
            )
            .Produces<UserSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapPost(
                "/{id:guid}/disable",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.disable"
                    );
                    var result = await service.Disable(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        id,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("DisableUser")
            .WithSummary("Turns an account off without deleting it.")
            .WithDescription(
                """
                Stops the person signing in. Nothing they created is touched, and the account can be turned back on with `POST /users/{id}/enable`.

                ### On success
                Returns `200 OK` with the `UserResponseDto`; `disabledAt` is set. Disabling an account that is already disabled succeeds and keeps the first time.

                ### Side effects
                - Every refresh token of the account is revoked, so the person is signed out of every device. An access token already issued keeps working until it expires (up to ~15 minutes).
                - Signing in with the right password then returns `403 ACCOUNT_DISABLED`.
                - The person gets an email saying their account was disabled. Disabling an account that is already disabled sends nothing.

                ### Path parameters
                - `id` — the user's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no such user in this congregation.
                - `409 CANNOT_MODIFY_SELF` — you cannot disable your own account.
                - `409 LAST_ADMIN` — this is the only admin who can sign in.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapPost(
                "/{id:guid}/enable",
                async (Guid id, UserService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.enable"
                    );
                    var result = await service.Enable(
                        context.User.GetCongregationId(),
                        context.User.GetUserId(),
                        id,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("EnableUser")
            .WithSummary("Turns a disabled account back on.")
            .WithDescription(
                """
                Lets a disabled account sign in again. The person signs in with their usual password; the sessions that were ended stay ended.

                ### On success
                Returns `200 OK` with the `UserResponseDto`; `disabledAt` is null. Enabling an account that is not disabled succeeds and changes nothing.

                ### Path parameters
                - `id` — the user's id, as returned by the list and create endpoints.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no such user in this congregation.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
                    UserService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.user.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchUsers")
            .CacheOutput(p => p.NoCache())
            .WithSummary("Searches users by name.")
            .WithDescription(
                """
                Trigram-based similarity search over user names, scoped to the caller's congregation. Backed by PostgreSQL `pg_trgm`.

                ### When to use this
                For autocomplete and quick lookup when the client needs to find a user without knowing their ID. For structured filtering, use the list endpoint instead — this one has no filters beyond the query string.

                ### Query parameter
                - `q` — required. The search string. Case-insensitive. Partial matches are supported — `"joh"` matches `"John"`.

                ### On success
                Returns `200 OK` with a flat array of `UserSearchResultDto`. **No pagination** — the result set is bounded server-side. Results are ranked by similarity to `q`, highest first.

                Each result includes `id`, `name`, and `emailAddress` — enough to render a picker without a follow-up call.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                This endpoint is rate-limited per the `search` policy. Excessive requests return `429 Too Many Requests`. Debounce the query in the UI.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<UserSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
