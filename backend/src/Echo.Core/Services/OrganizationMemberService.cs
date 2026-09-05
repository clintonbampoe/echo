using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class OrganizationMemberService(
    OrganizationMemberRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
)
    : PrimaryServiceBase<OrganizationMember, OrganizationMemberResponseDto>(
        repository,
        unitOfWork,
        mapper
    )
{
    private readonly OrganizationMemberRepository _organizationMemberRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _organizationMemberRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<OrganizationMemberListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _organizationMemberRepository.GetByIdAsync(id, congregationId, ct);

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
        var result = await _organizationMemberRepository.GetByMemberId(
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
        var result = await _organizationMemberRepository.GetByOrganizationId(
            paginationParameters,
            queryParameters,
            memberId,
            ct
        );

        return new SuccessResult<PagedResponse<OrganizationMemberListResponseDto>>(result);
    }
}
