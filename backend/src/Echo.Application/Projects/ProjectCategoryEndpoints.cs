using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Projects;

public static class ProjectCategoryEndpoints
{
    public static IEndpointRouteBuilder MapProjectCategoryEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/project-categories")
            .WithTags("Project Categories")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (ProjectCategoryService service, HttpContext context, CancellationToken ct) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListProjectCategories")
            .WithSummary("Returns all project categories for the congregation.")
            .WithDescription(
                """
                Returns the complete list of project categories for the authenticated congregation. Project categories are lightweight lookup entities — no pagination is applied and the full list is always returned.

                Fetch this list to populate category selectors when creating or updating projects.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<ProjectCategoryResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetProjectCategoryById")
            .WithSummary("Returns a single project category by ID.")
            .WithDescription(
                """
                Returns the project category record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    ProjectCategoryCreateDto dto,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateProjectCategory")
            .WithSummary("Creates a new project category.")
            .WithDescription(
                """
                Creates a new project category scoped to the authenticated congregation. Once created, the category becomes available for assignment to projects.

                On success, returns `201 Created` with the full category record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryUpdateDto dto,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.update"
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
            .WithName("UpdateProjectCategory")
            .WithSummary("Updates an existing project category.")
            .WithDescription(
                """
                Replaces the fields of an existing project category. Existing projects assigned to this category retain their assignment after the update.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<ProjectCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    int id,
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteProjectCategory")
            .WithSummary("Soft deletes a project category.")
            .WithDescription(
                """
                Marks the project category as deleted. The record is retained in the database but excluded from all list, search, and lookup results. Existing projects that reference this category are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no project category exists with the given ID in this congregation, or it has already been soft-deleted.
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
                    ProjectCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.project_category.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchProjectCategories")
            .WithSummary("Searches project categories by name.")
            .WithDescription(
                """
                Performs a trigram-based similarity search against project category names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<ProjectCategorySearchResultDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
