using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/users").WithTags("Users").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] PaginationRequest pagination,
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

                ### Ordering
                Users are ordered by name, then by ID as a tiebreaker. The order is stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter on the next request. When `hasMore` is `false`, `next` is null and no further pages exist.

                ### Authentication
                Bearer token required. The list is automatically scoped to the caller's congregation — there is no parameter to override this.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Not rate-limited beyond the platform default.
                """
            )
            .Produces<PagedResponse<UserResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has been soft-deleted. Both cases return the same code — the API does not reveal whether a record exists elsewhere.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    UserCreateDto dto,
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
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UserUpdateDto dto,
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
                - If the email changed, the account's verified state is **preserved as-is** — this endpoint does not reset verification. If your policy requires re-verification on email change, that must be handled separately.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has been soft-deleted.
                """
            )
            .Produces<UserResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

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
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
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
                - **Existing sessions are not revoked by this endpoint.** Any access token already issued continues to work until it expires (up to ~15 minutes). Any refresh token already issued can still be exchanged for a new pair. If you need to force a sign-out, call `POST /auth/sessions/logout` with the user's email **before** deleting.
                - Records that reference this user — as an event organizer, project manager, etc. — are not affected. The references remain, though lookups of the underlying user will now return `404`.

                ### Reversibility
                Soft delete is not exposed as reversible through the API. Restoring a deleted user requires database access.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no user exists with the given ID in this congregation, or the account has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery] string q,
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
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
