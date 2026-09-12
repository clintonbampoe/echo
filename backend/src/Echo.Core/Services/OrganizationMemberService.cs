using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.OrganizationMemberMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class OrganizationMemberService(
    OrganizationMemberRepository repository,
    OrganizationRepository organizationRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IOrganizationMemberMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        OrganizationMemberFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);
        var entities = await repository.GetPage(
            congregationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<OrganizationMemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

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
        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);
        var entities = await repository.ListByMemberId(
            congregationId,
            memberId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
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
        var cursor = encoder.Decode<OrganizationMemberCursor>(pagination.Cursor);
        var entities = await repository.ListByOrganizationId(
            congregationId,
            organizationId,
            filters,
            cursor,
            pagination.PageSize + 1,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var nextCursor = hasMore ? encoder.Encode(BuildCursor(entities.Last())) : null;

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<OrganizationMemberResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        OrganizationMemberCreateDto dto,
        CancellationToken ct
    )
    {
        var member = await memberRepository.GetById(congregationId, dto.MemberId, ct);
        if (member is null)
            return new ForeignKeyEntityNotFound(nameof(member));

        var organization = await organizationRepository.GetById(
            congregationId,
            dto.OrganizationId,
            ct
        );
        if (organization is null)
            return new ForeignKeyEntityNotFound(nameof(organization));

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.Member = member;
        entity.Organization = organization;

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

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
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    private OrganizationMemberCursor BuildCursor(OrganizationMember last)
    {
        return new OrganizationMemberCursor { CreatedAt = last.CreatedAt, Id = last.Id };
    }
}
