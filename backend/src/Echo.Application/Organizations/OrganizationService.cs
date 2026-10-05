using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Organizations;

public class OrganizationService(
    OrganizationRepository repository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IOrganizationMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<OrganizationService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        OrganizationFilters filters,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.organization.list");

        var cursor = encoder.Decode<OrganizationCursor>(pagination.Cursor);

        List<Organization> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.list"))
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
        activity?.SetTag("organization.count", data.Count);
        OrganizationLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<OrganizationResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.get_by_id"
        );
        activity?.SetTag("organization.id", id);

        Organization? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.organization.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            OrganizationLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        OrganizationLog.Found(logger, congregationId, id);
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
        OrganizationLog.Created(logger, congregationId, entity.Id);

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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            OrganizationLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.organization.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        OrganizationLog.Updated(logger, congregationId, id);

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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("organization.found", false);
            OrganizationLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.organization.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        OrganizationLog.Deleted(logger, congregationId, id);

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
        OrganizationLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<OrganizationSearchResultDto>>(res);
    }

    public async Task<IOperationResult> Summary(
        Guid congregationId,
        OrganizationFilters filters,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.summary"
        );

        var countTask = repository.Count(congregationId, filters, ct);
        var totalMembersTask = repository.CountTotalMembers(congregationId, filters, ct);
        var averageTask = repository.AverageMembersPerOrganization(congregationId, filters, ct);
        var largestTask = repository.LargestOrganization(congregationId, filters, ct);

        await Task.WhenAll(countTask, totalMembersTask, averageTask, largestTask);

        var res = new OrganizationSummaryDto
        {
            TotalOrganizations = countTask.Result,
            TotalMembers = totalMembersTask.Result,
            AverageMembersPerOrganization = averageTask.Result,
            LargestOrganization = largestTask.Result,
        };

        activity?.SetTag("organization.summary.total", res.TotalOrganizations);
        OrganizationLog.Summarized(logger, congregationId, res.TotalOrganizations);

        return new SuccessResult<OrganizationSummaryDto>(res);
    }

    private static OrganizationCursor BuildCursor(Organization last)
    {
        return new OrganizationCursor { Name = last.Name, Id = last.Id };
    }
}
