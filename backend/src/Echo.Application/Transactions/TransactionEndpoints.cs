using Echo.Shared.Extensions;
using Echo.Shared.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

                ### Paging
                - `cursor` — leave it out for the first page. For the next page, send the `next` value from the previous response unchanged. It records where the last page stopped, so use it only with the same filters it came from.
                - `pageSize` — how many rows to return, from 1 to 50. Defaults to 50; a larger number is reduced to 50.

                Keep asking for the next page while `hasMore` is `true`. When it is `false`, `next` is `null` and that was the last page.

                ### More filters
                - `paymentMethod` — only transactions paid this way.
                - `search` — partial, case-insensitive match on the description or the category name, so a user can type part of what they remember.

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
                    [FromRoute] Guid id,
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

                ### Path parameters
                - `id` — the transaction's id, as returned by the list and create endpoints.

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
                    [FromBody] TransactionCreateDto dto,
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

                ### Payment method
                - `paymentMethod` — how the money moved: `Cash`, `Cheque`, `CreditCard`, `MobileMoney` or `BankTransfer`. Optional; leave it out when unknown.

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
                    [FromRoute] Guid id,
                    [FromBody] TransactionUpdateDto dto,
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

                ### Path parameters
                - `id` — the transaction's id, as returned by the list and create endpoints.

                ### Payment method
                - `paymentMethod` — how the money moved: `Cash`, `Cheque`, `CreditCard`, `MobileMoney` or `BankTransfer`. Optional; leave it out when unknown.

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
                    [FromRoute] Guid id,
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

                ### Path parameters
                - `id` — the transaction's id, as returned by the list and create endpoints.

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
                - `byCategory` — one row per category and transaction type with `categoryId`, `categoryName`, `type`, `total` and `count`, sorted by `total` (largest first). Use it to see where the money goes.

                ### Filters
                All optional, and the same as on the list endpoint, so a summary can describe exactly what the list shows.
                - `transactionType` — `Income` or `Expense`.
                - `paymentMethod` — only transactions paid this way.
                - `search` — partial, case-insensitive match on the description or the category name.
                - `from` / `to` — the window. Also sets the window `previousTotalIncome` and `previousTotalExpenses` are compared against.

                ### Failure modes
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<TransactionSummaryDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapGet(
                "/summary/monthly",
                async (
                    [AsParameters] TransactionMonthlyFilters filters,
                    TransactionService service,
                    HttpContext context,
                    CancellationToken ct
                ) =>
                {
                    using var span = EndpointInstrumentation.StartSpan(
                        context,
                        instrumentation,
                        "endpoint.transaction.monthly_summary"
                    );
                    var result = await service.MonthlySummary(
                        context.User.GetCongregationId(),
                        filters,
                        ct
                    );
                    return result.ToResult();
                }
            )
            .WithName("GetTransactionMonthlySummary")
            .WithSummary("Returns income and expenses per month.")
            .WithDescription(
                """
                Returns one row per calendar month, so the Finance page can draw income against expenses, a net trend or a donut from a single request.

                Transactions are grouped by their `transactionDate`.

                ### Query parameters
                - `months` — how many months to return, from 1 to 12. Defaults to 12. The rows end with the month of `to`, so `months=6` is the last six months.
                - `to` — any day in the last month to return. Defaults to today, so the last row is this month so far (a part month). Send an earlier date to look back in time: `months=6&to=2026-03-15` returns October 2025 to March 2026.
                - `categoryId` — only transactions filed under this category.
                - `transactionType` — only `Income` or only `Expense`.

                ### Response
                A list of `{ year, month, totalIncome, totalExpenses, net }`, where `net` is `totalIncome` minus `totalExpenses` (negative when more went out than came in). Oldest month first. A month with nothing in it is returned with zeros, so there are no gaps and the list always has exactly `months` rows.

                ### Why it is one endpoint
                This is the only monthly endpoint for transactions: it serves a dashboard card (`months=6`), a full-year chart, or a donut over the months. The shape is always the same list, so the page decides how to draw it.

                ### Caching
                The answer is cached for 2 minutes per congregation and per set of parameters, so asking again straight away costs nothing. A change shows up within 2 minutes.

                ### Failure modes
                - `400 BAD_REQUEST` — `months` is not between 1 and 12.
                - `401 UNAUTHORIZED` — missing or invalid bearer token.
                """
            )
            .Produces<List<TransactionMonthlyDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
