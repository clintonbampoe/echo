using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.OrganizationMemberMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class OrganizationMemberService(
    OrganizationMemberRepository repository,
    OrganizationRepository organizationRepository,
    MemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IOrganizationMemberMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        Parameters? queryParameters,
        CancellationToken ct
    )
    {
        var entities = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, congregationId, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> GetByMemberId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var entities = await repository.GetByMemberId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<OrganizationMemberResponseDto>>(res);
    }

    public async Task<IOperationResult> GetByOrganizationId(
        PaginationParameters paginationParameters,
        Parameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var entities = await repository.GetByOrganizationId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        var res = mapper.ToListDto(entities);
        return new SuccessResult<List<OrganizationMemberResponseDto>>(res);
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
}
