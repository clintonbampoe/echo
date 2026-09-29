using Echo.Application.Members;
using Echo.Data;
using Echo.Domain.Members;
using Echo.Domain.Organizations;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;

namespace Echo.Application.Organizations;

public class OrganizationMemberService(
    OrganizationMemberRepository repository,
    OrganizationRepository organizationRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IOrganizationMemberMapper mapper,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        OrganizationMemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.org_member.list");

        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);

        List<OrganizationMember> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.list"))
        {
            entities = await repository.GetPage(
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
        activity?.SetTag("org_member.count", data.Count);

        var res = new PagedResponse<OrganizationMemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.org_member.get_by_id"
        );
        activity?.SetTag("org_member.id", id);

        OrganizationMember? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("org_member.found", false);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> ListByMemberId(
        Guid congregationId,
        Guid memberId,
        OrganizationMemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.org_member.list_by_member"
        );

        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);

        List<OrganizationMember> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.by_member"))
        {
            entities = await repository.ListByMemberId(
                congregationId,
                memberId,
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
        activity?.SetTag("org_member.count", data.Count);

        var res = new PagedResponse<OrganizationMemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> ListByOrganizationId(
        Guid congregationId,
        Guid organizationId,
        OrganizationMemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity(
            "svc.org_member.list_by_org"
        );

        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);

        List<OrganizationMember> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.by_org"))
        {
            entities = await repository.ListByOrganizationId(
                congregationId,
                organizationId,
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
        activity?.SetTag("org_member.count", data.Count);

        var res = new PagedResponse<OrganizationMemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        OrganizationMemberCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.org_member.create");

        Member? member;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.validate.member"))
        {
            member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        }
        if (member is null)
            return new ForeignKeyEntityNotFound(nameof(member));

        Organization? organization;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.validate.org"))
        {
            organization = await organizationRepository.GetById(
                congregationId,
                dto.OrganizationId,
                ct
            );
        }
        if (organization is null)
            return new ForeignKeyEntityNotFound(nameof(organization));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;
        entity.Organization = organization;

        using (instrumentation.ActivitySource.StartActivity("svc.org_member.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("org_member.id", entity.Id);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        OrganizationMemberUpdateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.org_member.update");
        activity?.SetTag("org_member.id", id);

        OrganizationMember? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("org_member.found", false);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);

        using (instrumentation.ActivitySource.StartActivity("svc.org_member.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.org_member.delete");
        activity?.SetTag("org_member.id", id);

        OrganizationMember? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.org_member.fetch.by_id"))
        {
            entity = await repository.GetById(id, congregationId, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("org_member.found", false);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.org_member.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        return new NoContentResult();
    }

    private static OrganizationMemberCursor BuildCursor(OrganizationMember last)
    {
        return new OrganizationMemberCursor { CreatedAt = last.CreatedAt, Id = last.Id };
    }
}
