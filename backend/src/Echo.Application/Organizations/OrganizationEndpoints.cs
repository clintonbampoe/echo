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
                Returns a cursor-paginated list of organizations scoped to the authenticated congregation.

                ### Filtering
                All filters are optional and combinable.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
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
                Returns the full organization record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or it has been soft-deleted.
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
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateOrganization")
            .WithSummary("Creates a new organization.")
            .WithDescription(
                """
                Creates a new organization scoped to the authenticated congregation. Once created, members can be assigned to it via the Organization Members resource.

                On success, returns `201 Created` with the full organization record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
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
                Replaces the fields of an existing organization record. All updatable fields must be supplied. Existing member assignments for this organization are not affected.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or it has been soft-deleted.
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
                Marks the organization as deleted. The record is retained in the database but excluded from all list, search, and lookup results. Existing member assignments linked to this organization are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no organization exists with the given ID in this congregation, or it has already been soft-deleted.
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
                Performs a trigram-based similarity search against organization names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
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
                Returns aggregated metrics for organizations in the congregation, optionally scoped by filters.

                ### Response includes
                - `totalOrganizations` — count of active organizations matching the filter.
                - `totalMembers` — total member assignments across matching organizations.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<OrganizationSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
