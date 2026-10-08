using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Transactions;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(
        this IEndpointRouteBuilder app,
        ApplicationInstrumentation instrumentation
    )
    {
        var group = app.MapGroup("/transactions").WithTags("Transactions").RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] TransactionFilters filters,
                    [AsParameters] PaginationRequest pagination,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.list"
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
            .WithName("ListTransactions")
            .WithSummary("Returns a paginated list of transactions.")
            .WithDescription(
                """
                Returns a cursor-paginated list of Transactions scoped to the caller's congregation.

                ### Filtering
                - `transactionType` — `Income` or `Expense`.
                - `categoryId` — filter to a specific Transaction Category.
                - `from` / `to` — filter by transaction date range.

                ### Ordering
                Transactions are ordered by transaction date, most recent first. Stable across pages.

                ### Pagination
                Pass the `next` cursor from the previous response as the `cursor` query parameter. When `hasMore` is `false`, `next` is null.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<PagedResponse<TransactionResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.fetch_by_id"
                    );
                    var result = await service.GetById(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("GetTransactionById")
            .WithSummary("Returns a single transaction by ID.")
            .WithDescription(
                """
                Returns the full Transaction record for the given ID, scoped to the caller's congregation.

                The response includes the resolved `categoryName` — no follow-up call needed.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction exists with the given ID in this congregation, or the record has been soft-deleted.
                """
            )
            .Produces<TransactionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/",
                async (
                    TransactionCreateDto dto,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.create"
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
            .WithName("CreateTransaction")
            .WithSummary("Records a new transaction.")
            .WithDescription(
                """
                Records a new Transaction — general income or expense.

                ### Request body
                Required fields:
                - `categoryId` — must reference a Transaction Category in the caller's congregation.
                - `transactionType` — `Income` or `Expense`.
                - `transactionDate` — ISO date.
                - `amount` — 0.01 to 1,000,000.

                Optional:
                - `description` — up to 2000 characters.

                ### The category type must match
                Transaction Categories are typed — each category is either an `Income` or `Expense` category. The category's `categoryType` and the transaction's `transactionType` should agree. **The API does not enforce this cross-check.** Sending an `Income` transaction with an `Expense` category is accepted — the two fields are validated independently. Filter the category picker on the transaction type the user is recording.

                ### Not attributed to a member
                Transactions have no member field. If you need giving attributed to a specific person, use **Tithes** (for tithes) or **Project Contributions** (for project-specific giving). Transactions are for general financial activity.

                ### On success
                Returns `201 Created` with the full `TransactionResponseDto` and a `Location` header pointing to `GET /transactions/{id}`.

                ### Side effects
                - The transaction is recorded. `GET /transactions/summary` reflects it on next call.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced category does not exist in this congregation.
                """
            )
            .Produces<TransactionResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    TransactionUpdateDto dto,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.update"
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
            .WithName("UpdateTransaction")
            .WithSummary("Updates an existing transaction.")
            .WithDescription(
                """
                Updates the supplied fields on an existing Transaction. Partial update.

                ### Request body
                All fields optional: `categoryId`, `transactionType`, `transactionDate`, `amount`, `description`.

                If `categoryId` is supplied, the new category is validated against the caller's congregation.

                ### On success
                Returns `200 OK` with the full updated `TransactionResponseDto`.

                ### Side effects
                - `GET /transactions/summary` recalculates on next call, reflecting the updated amount and category.

                ### Failure modes
                - `400 BAD_REQUEST` — the request body is malformed.
                - `400 VALIDATION_ERROR` — one or more fields failed validation.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction exists with the given ID in this congregation, or the record has been soft-deleted.
                - `404 FOREIGN_KEY_NOT_FOUND` — the referenced category does not exist in this congregation.
                """
            )
            .Produces<TransactionResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.delete"
                    );
                    var result = await service.Delete(context.User.GetCongregationId(), id, ct);
                    return result.ToResult();
                }
            )
            .WithName("DeleteTransaction")
            .WithSummary("Soft deletes a transaction.")
            .WithDescription(
                """
                Soft-deletes the Transaction. The row is retained in the database but excluded from list, summary, and lookup responses.

                ### On success
                Returns `204 No Content` with no body.

                ### Side effects
                - The transaction is marked as deleted.
                - **Summary totals reflect the deletion.** `GET /transactions/summary` recalculates `totalIncome`, `totalExpenses`, and `net` from live records only.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                - `404 NOT_FOUND` — no transaction exists with the given ID in this congregation, or the record has already been soft-deleted.
                """
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary",
                async (
                    [AsParameters] TransactionFilters filters,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.summary"
                    );
                    var result = await service.Summary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetTransactionSummary")
            .WithSummary("Returns aggregate summary metrics for transactions.")
            .WithDescription(
                """
                Returns aggregated financial metrics for the caller's congregation, optionally scoped by the same filters as the list endpoint.

                ### Response fields
                - `totalIncome` — sum of amounts for matching transactions with `transactionType = Income`.
                - `totalExpenses` — sum of amounts for matching transactions with `transactionType = Expense`.
                - `net` — `totalIncome - totalExpenses`. Negative when expenses exceed income.
                - `mostActiveCategory` — the category name with the largest total transaction volume (count, not amount). Null when no transactions match.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TransactionSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
