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
                Returns a cursor-paginated list of Projects scoped to the caller's congregation.

                ### Filtering
                All filters are optional and combinable:
                - `name` — partial, case-insensitive match against the project name.
                - `status` — `Planning`, `OnTrack`, `AtRisk`, `Complete`, `Missed`.
                - `categoryId` — filter to a specific Project Category.
                - `from` / `to` — filter by start-date range.

                ### Ordering
                Projects are ordered by start date, then by ID as a tiebreaker. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
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
                Returns the full Project record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `categoryName` and `managerName` — no follow-up calls needed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or the record has been soft-deleted.
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
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateProject")
            .WithSummary("Creates a new project.")
            .WithDescription(
                """
                Creates a new Project scoped to the caller's congregation.

                ### Request body
                Required fields:
                - `name` — 1 to 100 characters.
                - `categoryId` — must reference a Project Category in the caller's congregation.
                - `managerId` — must reference a Member in the caller's congregation.
                - `targetAmount` — 0.01 to 1,000,000.
                - `status` — `Planning`, `OnTrack`, `AtRisk`, `Complete`, `Missed`.
                - `startDate` — required.

                Optional fields:
                - `endDate` — optional; null for open-ended projects.
                - `description` — up to 2000 characters.

                ### On success
                Returns `201 Created` with the full `ProjectResponseDto` and a `Location` header pointing to `GET /projects/{id}`.

                ### Side effects
                - A new Project is created. **No contributions are created automatically.** To record money raised, use `POST /project-contributions`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced category or manager does not exist in this congregation.
                """
            )
            .Produces<ProjectResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

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
                Updates the supplied fields on an existing Project. Partial update — omitted fields retain their current values.

                ### Request body
                All fields optional. If `categoryId` or `managerId` is supplied, the new value is validated against the caller's congregation.

                ### On success
                Returns `200 OK` with the full updated `ProjectResponseDto`.

                ### Side effects
                - **Existing contributions linked to this project are not affected.** Changing the target amount or status does not rewrite contribution history.
                - Changing the target amount updates the projection for "raised vs target" — the `GET /projects/summary` endpoint recalculates `totalTarget` and derived percentages from the new value.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or the record has been soft-deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced category or manager does not exist in this congregation.
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
                Soft-deletes the Project. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The project is marked as deleted.
                - **Existing Project Contributions are not affected.** They remain in the database and continue to reference the (now deleted) project.
                - **New contributions cannot reference a deleted project** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.
                - Project summary totals (`GET /projects/summary`) no longer include the deleted project.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                Trigram-based similarity search over project names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive. Partial matches supported.

                ### On success
                Returns `200 OK` with a flat array of `ProjectSearchResultDto`, ranked by similarity. **No pagination.**

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
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
                Returns aggregated project metrics for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalProjects` — count of projects matching the filter.
                - `totalRaised` — sum of contributions across matching projects.
                - `totalTarget` — sum of target amounts across matching projects.
                - `atRiskCount` — count of projects with `status = AtRisk`.

                `totalRaised` is derived live from the Project Contributions resource, so a contribution deleted or added will be reflected immediately.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
