using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Echo.Shared.Utilities;

namespace Echo.Application.Transactions;

public class TransactionService(
    TransactionRepository repository,
    TransactionCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITransactionMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    ILogger<TransactionService> logger
)
{
    internal static DateWindow? ResolveWindow(TransactionFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisWeekToDate(
            filters.From,
            filters.To,
            hasParent: false
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        TransactionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.list");

        var cursor = encoder.Decode<TransactionCursor>(pagination.Cursor);

        List<Transaction> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.list"))
        {
            entities = await repository.List(
                congregationId,
                filters,
                cursor,
                pagination.PageSize + 1,
                ct
            );
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("transaction.count", data.Count);
        TransactionLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<TransactionResponseDto>(hasMore, nextCursor, data, window);
        return new SuccessResult<PagedResponse<TransactionResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction.get_by_id"
        );
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            TransactionLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        TransactionLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TransactionCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.create");

        TransactionCategory? transactionCategory;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.validate.category"))
        {
            transactionCategory = await categoryRepository.GetById(
                congregationId,
                dto.CategoryId,
                ct
            );
        }
        if (transactionCategory is null)
        {
            TransactionLog.CreateCategoryNotFound(logger, congregationId, dto.CategoryId);
            return new ForeignKeyEntityNotFound(nameof(transactionCategory));
        }
        TransactionLog.CreateCategoryFound(logger, congregationId, dto.CategoryId);

        if (!TransactionRules.TypeMatches(dto.TransactionType, transactionCategory))
            return new BadRequestResult(TypeMismatch(dto.TransactionType, transactionCategory));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Category = transactionCategory;

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("transaction.id", entity.Id);
        TransactionLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetTransactionById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetTransactionById' is not registered.");
        return new CreatedResult<TransactionResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        TransactionUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.update");
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            TransactionLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        // Check the category the transaction will end up in (the new one, else the current one)
        // against the type it will end up with.
        var categoryId = dto.CategoryId ?? entity.CategoryId;
        TransactionCategory? category;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.validate.category"))
        {
            category = await categoryRepository.GetById(congregationId, categoryId, ct);
        }

        if (category is null)
        {
            // An unchanged category may have been deleted since; only a new one is a bad reference.
            if (dto.CategoryId.HasValue && dto.CategoryId.Value != entity.CategoryId)
            {
                TransactionLog.UpdateCategoryNotFound(logger, congregationId, dto.CategoryId.Value);
                return new ForeignKeyEntityNotFound(nameof(category));
            }
        }
        else
        {
            TransactionLog.UpdateCategoryFound(logger, congregationId, categoryId);
            var type = dto.TransactionType ?? entity.TransactionType;
            if (!TransactionRules.TypeMatches(type, category))
                return new BadRequestResult(TypeMismatch(type, category));
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        TransactionLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<TransactionResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.transaction.delete");
        activity?.SetTag("transaction.id", id);

        Transaction? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.transaction.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("transaction.found", false);
            TransactionLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.transaction.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        TransactionLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction.summary"
        );

        var byCategory = await repository.GetCategoryTotals(congregationId, filters, ct);
        var res = BuildSummary(byCategory) with { Window = window };

        activity?.SetTag("transaction.summary.income", res.TotalIncome);
        TransactionLog.Summarized(logger, congregationId, res.TotalIncome, res.TotalExpenses);

        return new SuccessResult<TransactionSummaryDto>(res);
    }

    private static TransactionCursor BuildCursor(Transaction last)
    {
        return new TransactionCursor { TransactionDate = last.TransactionDate, Id = last.Id };
    }

    public async Task<IOperationResult> MonthlySummary(
        Guid congregationId,
        TransactionFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction.monthly_summary"
        );

        var window = dateWindows.LastTwelveMonths(filters.From, filters.To, hasParent: false)!.Value;
        var months = MonthRange.Between(window.From, window.To);
        if (MonthRange.Problem(months) is { } problem)
            return new BadRequestResult(problem);

        filters = WholeMonths(filters, months);
        var rows = await repository.GetMonthlyTotals(congregationId, filters, ct);

        return new SuccessResult<List<TransactionMonthlyDto>>(FillMonths(months, rows));
    }

    // From and To may be any day in their month, but the first and last months are charted whole.
    internal static TransactionFilters WholeMonths(
        TransactionFilters filters,
        IReadOnlyList<YearMonth> months
    )
    {
        var (from, to) = MonthRange.Bounds(months);
        return filters with { From = from, To = to };
    }

    // Months without transactions are returned as zeros so a chart has no gaps.
    internal static List<TransactionMonthlyDto> FillMonths(
        IReadOnlyList<YearMonth> months,
        IReadOnlyList<TransactionMonthlyDto> rows
    ) =>
        months
            .Select(m =>
                rows.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)
                ?? new TransactionMonthlyDto { Year = m.Year, Month = m.Month }
            )
            .ToList();

    // Every total in the summary comes from the per-category rows, so one query is enough.
    internal static TransactionSummaryDto BuildSummary(
        IReadOnlyList<TransactionCategoryTotalDto> byCategory
    )
    {
        var income = byCategory.Where(c => c.Type == TransactionType.Income).Sum(c => c.Total);
        var expenses = byCategory.Where(c => c.Type == TransactionType.Expense).Sum(c => c.Total);
        var mostActiveCategory = byCategory
            .GroupBy(c => c.CategoryName)
            .OrderByDescending(group => group.Sum(c => c.Count))
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .FirstOrDefault();

        return new TransactionSummaryDto
        {
            TotalIncome = income,
            TotalExpenses = expenses,
            Net = income - expenses,
            MostActiveCategory = mostActiveCategory,
            ByCategory = byCategory.ToList(),
        };
    }

    private static string TypeMismatch(TransactionType type, TransactionCategory category) =>
        $"A {type} transaction cannot be filed under the {category.CategoryType} category '{category.Name}'.";
}
