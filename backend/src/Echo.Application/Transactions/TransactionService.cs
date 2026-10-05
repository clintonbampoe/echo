using Echo.Data;
using Echo.Domain.Transactions;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Transactions;

public class TransactionService(
    TransactionRepository repository,
    TransactionCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITransactionMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<TransactionService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        TransactionFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
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

        var res = new PagedResponse<TransactionResponseDto>(hasMore, nextCursor, data);
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
        return new CreatedAtResult<TransactionResponseDto>(res);
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

        // Validate Category if it's being changed
        if (dto.CategoryId != entity.CategoryId)
        {
            TransactionCategory? category;
            using (
                instrumentation.ActivitySource.StartActivity("svc.transaction.validate.category")
            )
            {
                category = await categoryRepository.GetById(congregationId, entity.CategoryId, ct);
            }

            if (category is null)
            {
                TransactionLog.UpdateCategoryNotFound(logger, congregationId, entity.CategoryId);
                return new ForeignKeyEntityNotFound(nameof(category));
            }
            TransactionLog.UpdateCategoryFound(logger, congregationId, entity.CategoryId);
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
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.transaction.summary"
        );

        var incomeTask = repository.SumIncome(congregationId, filters, ct);
        var expensesTask = repository.SumExpenses(congregationId, filters, ct);
        var categoryTask = repository.MostActiveCategory(congregationId, filters, ct);

        await Task.WhenAll(incomeTask, expensesTask, categoryTask);

        var res = new TransactionSummaryDto
        {
            TotalIncome = incomeTask.Result,
            TotalExpenses = expensesTask.Result,
            Net = incomeTask.Result - expensesTask.Result,
            MostActiveCategory = categoryTask.Result,
        };

        activity?.SetTag("transaction.summary.income", res.TotalIncome);
        TransactionLog.Summarized(logger, congregationId, res.TotalIncome, res.TotalExpenses);

        return new SuccessResult<TransactionSummaryDto>(res);
    }

    private static TransactionCursor BuildCursor(Transaction last)
    {
        return new TransactionCursor { TransactionDate = last.TransactionDate, Id = last.Id };
    }
}
