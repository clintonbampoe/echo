using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Organizations;

public class OrganizationService(
    OrganizationRepository repository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IOrganizationMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.organization.list");

        var cursor = encoder.Decode<OrganizationCursor>(pagination.Cursor);

        List<Organization> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.list"))
        {
            entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);
        }

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        activity?.SetTag("organization.count", data.Count);

        var res = new PagedResponse<OrganizationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.get_by_id"
        );
        activity?.SetTag("organization.id", id);

        Organization? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        OrganizationCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.create"
        );

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        using (instrumentation.ActivitySource.StartActivity("svc.organization.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("organization.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<OrganizationResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        OrganizationUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.update"
        );
        activity?.SetTag("organization.id", id);

        Organization? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.organization.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.delete"
        );
        activity?.SetTag("organization.id", id);

        Organization? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.organization.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.search"
        );
        activity?.SetTag("organization.query", name);

        List<Organization> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.search"))
        {
            entities = await repository.Search(congregationId, name, ct);
        }

        var res = mapper.ToSearchDto(entities);
        activity?.SetTag("organization.count", res.Count);
        return new SuccessResult<List<OrganizationSearchResultDto>>(res);
    }

    private static OrganizationCursor BuildCursor(Organization last)
    {
        return new OrganizationCursor { Name = last.Name, Id = last.Id };
    }
}
