using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Members;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/members").WithTags("Members").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] MemberFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.list"
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
            .WithName("ListMembers")
            .WithSummary("Returns a paginated list of members.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Members scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:

                - `name` — partial, case-insensitive match against the member's full name.
                - `status` — `Active`, `Inactive`, `Archived`, `Transferred`.
                - `gender` — `Male`, `Female`, `Other`.
                - `region` — one of the sixteen Ghana regions (`Ashanti`, `GreaterAccra`, …).
                - `maritalStatus` — `Single`, `Married`, `Widowed`.
                - `from` / `to` — filter by joined-date range (inclusive).

                Omitting all filters returns every active member.

                ### Ordering
                Members are ordered by name, then by ID as a tiebreaker. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<MemberResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (Guid id, MemberService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetMemberById")
            .WithSummary("Returns a single member by ID.")
            .WithDescription(
                """
                Returns the full member profile for the given ID, scoped to the caller's congregation. Includes personal details, contact information, next of kin, and membership status.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    MemberCreateDto dto,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.create"
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
            .WithName("CreateMember")
            .WithSummary("Creates a new member.")
            .WithDescription(
                """
                Creates a new Member record scoped to the caller's congregation. This is the direct path — no visitor conversion involved.

                If you have a Visitor record and want to promote that person to a Member, use `POST /visitors/{id}/convert` instead. That endpoint preserves the existing person and history; this one creates a fresh record.

                ### Request body
                Required fields:
                - `firstName`, `lastName`
                - `phoneNumber`
                - `residentialAddress`, `city`, `hometown`
                - `nextOfKin`
                - `emergencyContactName`, `emergencyContactPhoneNumber`

                Optional fields:
                - `otherNames`, `emailAddress`
                - `dateOfBirth`, `joinedDate`
                - `gender`, `region`, `maritalStatus`, `gpsAddress`, `status`

                ### On success
                Returns `201 Created` with the full `MemberResponseDto` and a `Location` header pointing to `GET /members/{id}`.

                ### Side effects
                - A new Person record and Member record are created in the caller's congregation.
                - No user account is created — a Member is not the same as a User. If this person needs to sign in to Echo, create a User separately.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    MemberUpdateDto dto,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.update"
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
            .WithName("UpdateMember")
            .WithSummary("Updates an existing member.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Member. This is a partial update — omitted fields retain their current values.

                ### Request body
                All fields are optional. Supply only what you intend to change. The DTO mirrors `MemberCreateDto` but every field accepts null.

                ### On success
                Returns `200 OK` with the full updated `MemberResponseDto`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<MemberResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (Guid id, MemberService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteMember")
            .WithSummary("Soft deletes a member.")
            .WithDescription(
                """
                Soft-deletes the Member **and** the underlying Person record. The rows are retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The Member is marked as deleted.
                - The Person record backing the Member is also soft-deleted.
                - **Records linked to the member are not affected.** Attendance, organization memberships, tithe records, event registrations, and event attendance all remain in the database and continue to reference the deleted member. Queries that join to the member will see it as missing.
                - **User accounts are not affected.** If a User account exists that happens to correspond to this person, it remains active. The two records are not linked.

                ### Reversibility
                Not exposed as reversible through the API.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no member exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchMembers")
            .WithSummary("Searches members by name.")
            .WithDescription(
                """
                Trigram-based similarity search against member names using `pg_trgm`. Searches across full name, first name, and last name — a query of `"Ama Mensah"` matches a member named that, and a query of `"Mensah"` matches any member with that surname.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `MemberSearchResultDto`, ranked by similarity. **No pagination.** Each result includes `id`, `name`, and `phoneNumber` — enough to disambiguate common names without a follow-up call.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy. Debounce the query in the UI.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<MemberSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] MemberFilters filters,
                    MemberService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.member.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetMemberSummary")
            .WithSummary("Returns aggregate summary metrics for members.")
            .WithDescription(
                """
                Returns aggregated membership counts for the caller's congregation, optionally scoped by the same filter set as the list endpoint.

                ### Response fields
                - `totalMembers` — count of members matching the filter.
                - `activeMembers` — count of members with `status = Active`.
                - `maleCount` — count with `gender = Male`.
                - `femaleCount` — count with `gender = Female`.
                - `averageAge` — mean age in years, computed from `dateOfBirth`.

                Age is calculated against the current date; members with no `dateOfBirth` are excluded from the average but still counted in the totals.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<MemberSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
