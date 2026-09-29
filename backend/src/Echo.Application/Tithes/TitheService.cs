using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Tithes;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Tithes;

public class TitheService(
    TitheRepository repository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    ITitheMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        TitheFilter filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.list");

        var cursor = encoder.Decode<TitheCursor>(pagination.Cursor);

        List<Tithe> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.list"))
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
        activity?.SetTag("tithe.count", data.Count);

        var res = new PagedResponse<TitheResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<TitheResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.get_by_id");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        TitheCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.create");

        Member? member;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.validate.member"))
        {
            member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        }
        if (member is null)
            return new ForeignKeyEntityNotFound(nameof(member));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("tithe.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        TitheUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.update");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            return new NotFoundResult(id.ToString());
        }
        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<TitheResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.tithe.delete");
        activity?.SetTag("tithe.id", id);

        Tithe? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.tithe.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("tithe.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.tithe.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    private static TitheCursor BuildCursor(Tithe last)
    {
        return new TitheCursor { CollectionDate = last.CollectionDate, Id = last.Id };
    }
}
