using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Projects;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/projects").WithTags("Projects").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ProjectFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.list"
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
            .WithName("ListProjects")
            .WithSummary("Returns a paginated list of projects.")
            .WithDescription(
                """
                Returns a cursor-paginated list of projects scoped to the authenticated congregation.

                ### Filtering
                Filter by category, status, or date range. All filters are optional and combinable.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<ProjectResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetProjectById")
            .WithSummary("Returns a single project by ID.")
            .WithDescription(
                """
                Returns the full project record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    ProjectCreateDto dto,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateProject")
            .WithSummary("Creates a new project.")
            .WithDescription(
                """
                Creates a new project scoped to the authenticated congregation. The project must reference an existing Project Category within the same congregation.

                On success, returns `201 Created` with the full project record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced Project Category does not exist in this congregation.
                """
            )
            .Produces<ProjectResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectUpdateDto dto,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.update"
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
            .WithName("UpdateProject")
            .WithSummary("Updates an existing project.")
            .WithDescription(
                """
                Replaces the fields of an existing project. All updatable fields must be supplied. Existing contributions linked to this project are not affected.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteProject")
            .WithSummary("Soft deletes a project.")
            .WithDescription(
                """
                Marks the project as deleted. The record is retained in the database but excluded from all list, search, and lookup results. Existing contributions linked to this project are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or it has already been soft-deleted.
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
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchProjects")
            .WithSummary("Searches projects by name.")
            .WithDescription(
                """
                Performs a trigram-based similarity search against project names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<ProjectSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] ProjectFilters filters,
                    ProjectService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetProjectSummary")
            .WithSummary("Returns aggregate summary metrics for projects.")
            .WithDescription(
                """
                Returns aggregated project metrics for the congregation, optionally scoped by filters.

                ### Response includes
                - `totalProjects` — count of active projects matching the filter.
                - `totalFundingGoal` — sum of funding goals across matching projects.
                - `totalRaised` — sum of contributions received across matching projects.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
