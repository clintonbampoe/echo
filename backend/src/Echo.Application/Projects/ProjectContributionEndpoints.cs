using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Projects;

public static class ProjectContributionEndpoints
{
    public static IEndpointRouteBuilder MapProjectContributionEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/project-contributions")
            .WithTags("Project Contributions")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ProjectContributionFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.list"
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
            .WithName("ListProjectContributions")
            .WithSummary("Returns a paginated list of project contributions.")
            .WithDescription(
                """
                Returns a cursor-paginated list of project contributions scoped to the authenticated congregation.

                ### Filtering
                Filter by project, member, or date range. All filters are optional and combinable.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter to fetch the next page. When `hasMore` is false no further pages exist.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<ProjectContributionResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetProjectContributionById")
            .WithSummary("Returns a single project contribution by ID.")
            .WithDescription(
                """
                Returns the full project contribution record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project contribution exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    ProjectContributionCreateDto dto,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateProjectContribution")
            .WithSummary("Records a new project contribution.")
            .WithDescription(
                """
                Records a financial or in-kind contribution made by a member toward a specific project. Both the member and the project must exist within the same congregation.

                On success, returns `201 Created` with the full contribution record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced member or project does not exist in this congregation.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectContributionUpdateDto dto,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.update"
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
            .WithName("UpdateProjectContribution")
            .WithSummary("Updates an existing project contribution.")
            .WithDescription(
                """
                Replaces the fields of an existing project contribution. All updatable fields must be supplied.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project contribution exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectContributionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteProjectContribution")
            .WithSummary("Soft deletes a project contribution.")
            .WithDescription(
                """
                Marks the project contribution as deleted. The record is retained in the database but excluded from all list and lookup results. Project summary totals will reflect the deletion.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project contribution exists with the given ID in this congregation, or it has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] ProjectContributionFilters filters,
                    ProjectContributionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_contribution.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetProjectContributionSummary")
            .WithSummary("Returns aggregate summary metrics for project contributions.")
            .WithDescription(
                """
                Returns aggregated contribution metrics for the congregation, optionally scoped by the same filters available on the list endpoint.

                ### Response includes
                - `totalContributions` — count of contributions matching the filter.
                - `totalAmount` — sum of all contribution amounts matching the filter.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectContributionSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
