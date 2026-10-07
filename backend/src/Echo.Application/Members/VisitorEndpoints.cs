using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Members;

public static class VisitorEndpoints
{
    public static IEndpointRouteBuilder MapVisitorEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/visitors").WithTags("Visitors").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] VisitorFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.list"
                    );
                    var result = await service.List(
                        context.User.GetCongregationId(),
                        filters,
                        pagination,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ListVisitors")
            .WithSummary("Returns a paginated list of visitors.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Visitors scoped to the caller's congregation.

                ### Filtering
                - `name` — partial, case-insensitive match against the visitor's full name.
                - `converted` — when `true`, returns only visitors who have been converted to Members. When `false`, returns only unconverted visitors. Omit to receive both.
                - `from` / `to` — filter by first-visit date range.

                ### Default behavior
                With no filters, the list includes **both** converted and unconverted visitors. This is different from what you might expect — the historical record is preserved and shows up by default. If you only want active visitors in a picker, pass `converted=false` explicitly.

                ### Ordering
                Ordered by name, then by ID as a tiebreaker. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<VisitorResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetVisitorById")
            .WithSummary("Returns a single visitor by ID.")
            .WithDescription(
                """
                Returns the full Visitor record for the given ID, scoped to the caller's congregation.

                If the visitor has been converted to a Member, the response includes `convertedToMemberPersonId`, `convertedToMemberName`, and `convertedAt`. You can use `convertedToMemberPersonId` to fetch the Member via `GET /members/{id}`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    VisitorCreateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.create"
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
            .WithName("CreateVisitor")
            .WithSummary("Records a new visitor.")
            .WithDescription(
                """
                Records a new Visitor for the caller's congregation. Use this when someone attends for the first time but is not yet a Member.

                ### Request body
                - `firstName`, `lastName` — required.
                - `phoneNumber`, `emailAddress` — optional.
                - `dateOfBirth` — optional.
                - `notes` — optional, up to 2000 characters. Free-form context — how they heard about the congregation, who invited them, follow-up reminders, anything the congregation wants to track.

                Visitors have a much smaller profile than Members by design. The rest of the profile — next of kin, emergency contact, residential address — is captured at conversion time, not here.

                ### On success
                Returns `201 Created` with the full `VisitorResponseDto` and a `Location` header pointing to `GET /visitors/{id}`.

                ### Side effects
                - A new Person record and Visitor record are created in the caller's congregation.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    VisitorUpdateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.update"
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
            .WithName("UpdateVisitor")
            .WithSummary("Updates an existing visitor.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Visitor. Partial update — omitted fields retain their current values.

                ### Request body
                All fields are optional. The DTO allows updating `firstName`, `lastName`, `phoneNumber`, `emailAddress`, `dateOfBirth`, and `notes`.

                **`convertedToMemberPersonId` and `convertedAt` are not editable.** Once a visitor has been converted, that state is permanent — you cannot un-convert a visitor through this endpoint.

                ### On success
                Returns `200 OK` with the full updated `VisitorResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<VisitorResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteVisitor")
            .WithSummary("Soft deletes a visitor.")
            .WithDescription(
                """
                Soft-deletes the Visitor and the underlying Person record. Both are excluded from list, search, and lookup responses but retained in the database.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The Visitor and Person rows are marked as deleted.
                - **Attendance records that reference the visitor are not affected.** If the visitor had been counted in any general attendance records, those records remain and continue to reference the (now deleted) person.
                - If the visitor was previously converted to a Member, the resulting Member record is **not** affected by this deletion. Deleting the visitor does not delete the member.

                ### When to use this
                - Someone was recorded as a visitor by mistake.
                - A duplicate visitor record needs to be removed.

                Not to be confused with conversion. If you want to promote the visitor to a Member, use `POST /visitors/{id}/convert` — do not delete the visitor.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchVisitors")
            .WithSummary("Searches visitors by name.")
            .WithDescription(
                """
                Trigram-based similarity search against visitor names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `VisitorSearchResultDto`, ranked by similarity. **No pagination.** Each result includes `id`, `name`, and `phoneNumber`.

                ### What is searched
                All visitor records are eligible — converted and unconverted alike. Converted visitors will appear in results. If your UI needs to filter them out, check the `converted` flag via `GET /visitors/{id}`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<VisitorSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/{id:guid}/convert",
                async (
                    Guid id,
                    MemberCreateDto dto,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.convert"
                    );
                    var result = await service.Convert(
                        context.User.GetCongregationId(),
                        id,
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("ConvertVisitorToMember")
            .WithSummary("Converts a visitor record into a full member.")
            .WithDescription(
                """
                Promotes an existing Visitor to a Member. The Visitor's Person record is preserved — the conversion flips its `Kind` from `Visitor` to `Member` and creates the Member side table.

                ### Request body
                A full `MemberCreateDto`. The service uses it to build the Member side table — the fields that a Visitor does not have (next of kin, emergency contact, residential address, and so on).

                ### On success
                Returns `201 Created` with the new `MemberResponseDto` and a `Location` header pointing to `GET /members/{id}`.

                ### Side effects
                - The Person's `Kind` flips from `Visitor` to `Member`.
                - A new Member row is created, linked to the existing Person.
                - The Visitor record is retained for history but marked converted: `convertedToMemberPersonId`, `convertedToMemberName`, and `convertedAt` are populated.
                - **The `id` of the new Member is the same UUID as the original visitor's Person ID.** If the client stored the visitor ID, it can be used as the member ID in subsequent requests — the underlying Person is the same entity.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no visitor exists with the given ID in this congregation, or the record has been soft-deleted.
                - `409 CONFLICT` — the visitor has already been converted. A visitor can only be converted once. The response includes the visitor ID — use `GET /visitors/{id}` to retrieve `convertedToMemberPersonId` and fetch the existing Member instead of re-attempting conversion.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] VisitorFilters filters,
                    VisitorService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.visitor.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetVisitorSummary")
            .WithSummary("Returns aggregate summary metrics for visitors.")
            .WithDescription(
                """
                Returns aggregated visitor counts for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalVisitors` — count of visitors matching the filter.
                - `newVisitors` — count of first-time visitors (no prior visit recorded).
                - `recurringVisitors` — count of visitors with more than one recorded visit.
                - `convertedVisitors` — count of visitors who have been converted to Members.

                `newVisitors + recurringVisitors` is not necessarily equal to `totalVisitors` — the classification depends on how the underlying repository defines "first visit," and the counts may overlap depending on filter scope.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<VisitorSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
