using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Transactions;

public static class TransactionCategoryEndpoints
{
    public static IEndpointRouteBuilder MapTransactionCategoryEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/transaction-categories")
            .WithTags("Transaction Categories")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.list"
                    );
                    var result = await service.List(context.User.GetCongregationId(), ct);
                    return result.ToResult();
                }
            )
            .WithName("ListTransactionCategories")
            .WithSummary("Returns all transaction categories for the congregation.")
            .WithDescription(
                """
                Returns the complete list of transaction categories for the authenticated congregation. Transaction categories are lightweight lookup entities — no pagination is applied and the full list is always returned.

                Fetch this list to populate category selectors when creating or updating transactions.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<TransactionCategoryResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:int}",
                async (
                    int id,
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetTransactionCategoryById")
            .WithSummary("Returns a single transaction category by ID.")
            .WithDescription(
                """
                Returns the transaction category record for the given ID, scoped to the authenticated congregation.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<TransactionCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    TransactionCategoryCreateDto dto,
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.create"
                    );
                    var result = await service.Create(context.User.GetCongregationId(), dto, ct);
                    return result.ToResult();
                }
            )
            .WithName("CreateTransactionCategory")
            .WithSummary("Creates a new transaction category.")
            .WithDescription(
                """
                Creates a new transaction category scoped to the authenticated congregation. Once created, it becomes available for assignment to transactions.

                On success, returns `201 Created` with the full category record and a `Location` header pointing to the newly created resource.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TransactionCategoryResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPut(
                "/{id:int}",
                async (
                    int id,
                    TransactionCategoryUpdateDto dto,
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.update"
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
            .WithName("UpdateTransactionCategory")
            .WithSummary("Updates an existing transaction category.")
            .WithDescription(
                """
                Replaces the fields of an existing transaction category. Existing transactions assigned to this category retain their assignment after the update.

                ### Errors
                - `400 BAD_REQUEST` — malformed request body.
                - `400 VALIDATION_ERROR` — one or more fields failed validation. Inspect the `errors` object.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or it has been soft-deleted.
                """
            )
            .Produces<TransactionCategoryResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:int}",
                async (
                    int id,
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteTransactionCategory")
            .WithSummary("Soft deletes a transaction category.")
            .WithDescription(
                """
                Marks the transaction category as deleted. The record is retained in the database but excluded from all list, search, and lookup results. Existing transactions that reference this category are not affected.

                Returns `204 No Content` on success.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or it has already been soft-deleted.
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
                    TransactionCategoryService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction_category.search"
                    );
                    var result = await service.Search(context.User.GetCongregationId(), q, ct);
                    return result.ToResult();
                }
            )
            .WithName("SearchTransactionCategories")
            .WithSummary("Searches transaction categories by name.")
            .WithDescription(
                """
                Performs a trigram-based similarity search against transaction category names using `pg_trgm`. Results are ranked by similarity to the query string `q`. Returns a flat list — no pagination.

                This endpoint is rate-limited. Excessive requests will be rejected.

                ### Errors
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<TransactionCategorySearchResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
