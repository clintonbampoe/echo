using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Organizations;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/organizations").WithTags("Organizations").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] OrganizationFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.list"
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
            .WithName("ListOrganizations")
            .WithSummary("Returns a paginated list of organizations.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Organizations scoped to the caller's congregation.

                ### Filtering
                - `name` — partial, case-insensitive match against the organization name.
                - `from` / `to` — filter by creation-date range.

                Omitting all filters returns every active organization.

                ### Ordering
                Organizations are ordered by name, then by ID as a tiebreaker. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<OrganizationResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetOrganizationById")
            .WithSummary("Returns a single organization by ID.")
            .WithDescription(
                """
                Returns the full Organization record for the given ID, scoped to the caller's congregation.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<OrganizationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    OrganizationCreateDto dto,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.create"
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
            .WithName("CreateOrganization")
            .WithSummary("Creates a new organization.")
            .WithDescription(
                """
                Creates a new Organization scoped to the caller's congregation.

                ### Request body
                - `name` — required. 1 to 100 characters.
                - `description` — optional, up to 2000 characters.

                ### On success
                Returns `201 Created` with the full `OrganizationResponseDto` and a `Location` header pointing to `GET /organizations/{id}`.

                ### Side effects
                - A new Organization is created.
                - No member assignments are made. To assign members, use `POST /organization-members` with the new Organization's ID.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — `name` failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<OrganizationResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationUpdateDto dto,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.update"
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
            .WithName("UpdateOrganization")
            .WithSummary("Updates an existing organization.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Organization. Partial update — omitted fields retain their current values.

                ### Request body
                All fields optional: `name`, `description`.

                ### On success
                Returns `200 OK` with the full updated `OrganizationResponseDto`.

                ### Side effects
                - **Existing member assignments for this organization are not affected.** The members remain in the organization under the updated name.
                - **Events that reference this organization are not affected.** They continue to link to the same Organization ID.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<OrganizationResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteOrganization")
            .WithSummary("Soft deletes an organization.")
            .WithDescription(
                """
                Soft-deletes the Organization. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The organization is marked as deleted.
                - **Existing member assignments are not affected.** Organization Member records that reference this organization remain in the database. When those assignments are fetched via their own endpoints, `organizationName` may resolve to null.
                - **Events that reference this organization are not affected.** They continue to link to the (now deleted) organization.
                - **New member assignments cannot reference a deleted organization** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchOrganizations")
            .WithSummary("Searches organizations by name.")
            .WithDescription(
                """
                Trigram-based similarity search over organization names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `OrganizationSearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<OrganizationSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] OrganizationFilters filters,
                    OrganizationService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.organization.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetOrganizationSummary")
            .WithSummary("Returns aggregate summary metrics for organizations.")
            .WithDescription(
                """
                Returns aggregated metrics for organizations in the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalOrganizations` — count of organizations matching the filter.
                - `totalMembers` — total member assignments across matching organizations.
                - `averageMembersPerOrganization` — mean of the member counts.
                - `largestOrganization` — name of the organization with the most members. Null if no organizations match.

                `totalMembers` counts **assignments**, not distinct members. A member who belongs to three organizations contributes three to this number.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<OrganizationSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
