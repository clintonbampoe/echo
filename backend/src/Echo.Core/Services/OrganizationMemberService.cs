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
    IUnitOfWork unitOfWork,
    IOrganizationMemberMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> GetPage(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetPage(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<OrganizationMemberListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Organization member not found.");

        return new SuccessResult<OrganizationMemberResponseDto>(result);
    }

    public async Task<IOperationResult> GetByMemberId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var result = await repository.GetByMemberId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        return new SuccessResult<PagedResponse<OrganizationMemberListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetByOrganizationId(
        PaginationParameters paginationParameters,
        QueryParameters queryParameters,
        Guid memberId,
        CancellationToken ct
    )
    {
        var result = await repository.GetByOrganizationId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        return new SuccessResult<PagedResponse<OrganizationMemberListResponseDto>>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, OrganizationMemberCreateDto dto,
        CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, OrganizationMemberUpdateDto dto,
        CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<OrganizationMemberResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetEntityById(congregationId, id, ct);

        if (entity is null)
            return new NotFoundResult(id.ToString());

        await repository.SoftDelete(entity, ct);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }
}
