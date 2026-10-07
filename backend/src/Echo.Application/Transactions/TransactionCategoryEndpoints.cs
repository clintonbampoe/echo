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
                Returns the complete list of Transaction Categories for the caller's congregation.

                ### No pagination
                Plain array, not a paged envelope.

                ### Categories are typed
                Each category carries a `categoryType` of `Income` or `Expense`. When building a category picker for a transaction, filter this list by the `categoryType` that matches the transaction type the user is recording — a Salary category (`Expense`) should not appear when the user is recording an Income transaction.

                ### Failure modes
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
                Returns the Transaction Category record for the given ID, scoped to the caller's congregation.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or the record has been soft-deleted.
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
                    var result = await service.Create(
                        context.User.GetCongregationId(),
                        dto,
                        context,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("CreateTransactionCategory")
            .WithSummary("Creates a new transaction category.")
            .WithDescription(
                """
                Creates a new Transaction Category — for example Offerings, Utilities, or Salaries. Scoped to the caller's congregation.

                ### Request body
                - `name` — required. 1 to 100 characters.
                - `categoryType` — **required.** `Income` or `Expense`. The type is set at creation and determines what the category can be used for.

                ### On success
                Returns `201 Created` with the full `TransactionCategoryResponseDto` and a `Location` header pointing to `GET /transaction-categories/{id}`.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
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
                Updates the name or type of a Transaction Category. Partial update.

                ### Request body
                - `name` — optional.
                - `categoryType` — optional. **Changing the type affects future transaction creation.** Existing transactions that reference this category keep their own `transactionType` — the category type and the transaction type can become inconsistent if you change one without the other. Prefer to leave the type alone after creation unless you have a specific need.

                ### Side effects
                - **Existing Transactions assigned to this category are not affected** and retain their assignment. They will display the updated category name on next fetch.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or the record has been soft-deleted.
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
                Soft-deletes the Transaction Category. The row is retained in the database but excluded from list, search, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The category is marked as deleted.
                - **Existing Transactions assigned to this category are not affected.** They keep their `categoryId` pointing at the deleted category. `categoryName` may resolve to null on fetch.
                - **New transactions cannot reference a deleted category** — the FK lookup will fail with `404 FOREIGN_KEY_NOT_FOUND`.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction category exists with the given ID in this congregation, or the record has already been soft-deleted.
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
                Trigram-based similarity search over transaction category names using `pg_trgm`.

                ### Query parameter
                - `q` — required. Case-insensitive.

                ### On success
                Returns `200 OK` with a flat array of `TransactionCategorySearchResponseDto`, ranked by similarity. **No pagination.**

                Each result includes the category's `type` (`Income` or `Expense`), so the client can group or filter results without a follow-up call.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.

                ### Rate limiting
                Rate-limited per the `search` policy.
                """
            )
            .RequireRateLimiting("search")
            .Produces<List<TransactionCategorySearchResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
