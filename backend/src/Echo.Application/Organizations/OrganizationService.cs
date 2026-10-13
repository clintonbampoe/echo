using Echo.Data;
using Echo.Domain.Organizations;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Echo.Shared.Utilities;

namespace Echo.Application.Organizations;

public class OrganizationService(
    OrganizationRepository repository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IOrganizationMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    DateWindows dateWindows,
    ILogger<OrganizationService> logger
)
{
    internal static DateWindow? ResolveWindow(OrganizationFilters filters, DateWindows dateWindows) =>
        dateWindows.ThisYear(
            filters.From,
            filters.To,
            hasParent: false
        );

    public async Task<IOperationResult> List(
        Guid congregationId,
        OrganizationFilters filters,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

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

        var counts = await repository.GetMemberCounts(
            congregationId,
            entities.Select(x => x.Id).ToList(),
            ct
        );
        var data = entities
            .Select(x => mapper.ToDto(x) with { MemberCount = counts.GetValueOrDefault(x.Id) })
            .ToList();
        activity?.SetTag("organization.count", data.Count);
        OrganizationLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<OrganizationResponseDto>(hasMore, nextCursor, data, window);
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
        var res = await WithCount(entity, congregationId, ct);
        return new SuccessResult<OrganizationResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        OrganizationCreateDto dto,
        HttpContext httpContext,
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

        var location =
            linker.GetPathByName(httpContext, "GetOrganizationById", new { id = res.Id })
            ?? throw new InvalidOperationException(
                "Route 'GetOrganizationById' is not registered."
            );
        return new CreatedResult<OrganizationResponseDto>(location, res);
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

        var res = await WithCount(entity, congregationId, ct);
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
        var window = ResolveWindow(filters, dateWindows);
        filters = filters with { From = window?.From, To = window?.To };

        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.organization.summary"
        );

        var stats = await repository.GetSummaryStats(congregationId, filters, ct);

        var res = new OrganizationSummaryDto
        {
            Window = window,
            TotalOrganizations = stats.Count,
            TotalMembers = stats.TotalMembers,
            AverageMembersPerOrganization = stats.AverageMembers,
            LargestOrganization = stats.Largest,
        };

        activity?.SetTag("organization.summary.total", res.TotalOrganizations);
        OrganizationLog.Summarized(logger, congregationId, res.TotalOrganizations);

        return new SuccessResult<OrganizationSummaryDto>(res);
    }

    private static OrganizationCursor BuildCursor(Organization last)
    {
        return new OrganizationCursor { Name = last.Name, Id = last.Id };
    }

    private async Task<OrganizationResponseDto> WithCount(Organization entity, Guid congregationId, CancellationToken ct)
    {
        var counts = await repository.GetMemberCounts(congregationId, [entity.Id], ct);
        return mapper.ToDto(entity) with { MemberCount = counts.GetValueOrDefault(entity.Id) };
    }
}
