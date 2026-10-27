using System.ComponentModel.DataAnnotations;
using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Attendances;

public static class SessionLabelEndpoints
{
    public static IEndpointRouteBuilder MapSessionLabelEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/attendance/labels")
            .WithTags("Session Labels")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (SessionLabelService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListSessionLabels")
            .WithSummary("Returns all session labels for the congregation.")
            .WithDescription(
                """
                Returns the complete list of session labels for the caller's congregation.

                ### What is a session label
                A session label is a named, congregation-scoped tag used to categorize sessions — "Sunday Service", "Midweek Service", "Prayer Meeting". Every session references exactly one label.

                Each label also carries a `category` — `Service`, `Meeting`, `Study`, `Prayer`, `Practice`, `Outreach`, `Class`, or `Special`. The category is fixed at creation and used for cross-congregation analytics. The `name` is what the congregation sees and can rename freely.

                ### No pagination
                The response is a plain array. Session labels are bounded in number and lightweight.

                ### Ordering
                Labels are returned alphabetically by name.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<SessionLabelResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    [FromRoute] int id,
                    SessionLabelService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetSessionLabelById")
            .WithSummary("Returns a single session label by ID.")
            .WithDescription(
                """
                Returns the session label record for the given ID, scoped to the caller's congregation.

                ### ID is an integer
                Session label IDs are 32-bit integers, not UUIDs. Lookup entities in Echo use integer keys for compactness.

                ### Path parameters
                - `id` — the session label's id (a whole number, not a GUID), as returned by `GET /attendance/labels`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no session label exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<SessionLabelResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    [FromBody] SessionLabelCreateDto dto,
                    SessionLabelService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.create"
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
            .WithName("CreateSessionLabel")
            .WithSummary("Creates a new session label.")
            .WithDescription(
                """
                Creates a new session label scoped to the caller's congregation.

                ### Request body
                - `name` — **required.** The display name. Minimum 1 character, maximum 100. Not enforced unique, but duplicate names are confusing in practice.
                - `category` — **required.** One of `Service`, `Meeting`, `Study`, `Prayer`, `Practice`, `Outreach`, `Class`, `Special`. The category is not editable after creation.

                ### On success
                Returns `201 Created` with the full `SessionLabelResponseDto` and a `Location` header pointing to `GET /attendance/labels/{id}`.

                ### Side effects
                - The label becomes immediately available when creating Sessions.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — a field failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<SessionLabelResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    [FromRoute] int id,
                    [FromBody] SessionLabelUpdateDto dto,
                    SessionLabelService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.update"
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
            .WithName("UpdateSessionLabel")
            .WithSummary("Updates an existing session label.")
            .WithDescription(
                """
                Updates a session label's name. Only `name` can be changed — the category is fixed at creation.

                ### Side effects
                - **Existing sessions that reference this label are not affected** and continue to work. They will now display the updated name in their `sessionLabelName` field.
                - Historical records are indistinguishable from records created after the rename — there is no versioning of the label name on sessions.

                ### Path parameters
                - `id` — the session label's id (a whole number, not a GUID), as returned by `GET /attendance/labels`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no session label exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<SessionLabelResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    [FromRoute] int id,
                    SessionLabelService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteSessionLabel")
            .WithSummary("Soft deletes a session label.")
            .WithDescription(
                """
                Soft-deletes the session label. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The label is marked as deleted.
                - **Existing sessions that reference this label are not affected.** They remain in the database with a foreign key to the (now deleted) label. `sessionLabelName` still returns the deleted label's name.
                - **New sessions cannot reference a deleted label** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### When to use this
                Only when a label is genuinely obsolete. If you just want to stop using a label, consider leaving it in place — the reference integrity cost of deletion is higher than the readability cost of a stale label in a dropdown.

                ### Path parameters
                - `id` — the session label's id (a whole number, not a GUID), as returned by `GET /attendance/labels`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no session label exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/search",
                async (
                    [FromQuery, StringLength(100, MinimumLength = 1)] string q,
                    SessionLabelService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.session_label.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchSessionLabels")
            .WithSummary("Searches session labels by name.")
            .WithDescription(
                """
                Trigram-based similarity search over session label names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `SessionLabelSearchResultDto`, ranked by similarity. No pagination.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<SessionLabelSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
