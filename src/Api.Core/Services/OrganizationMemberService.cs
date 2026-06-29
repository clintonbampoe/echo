using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Dtos.Core;
using Api.Core.Entities;
using Api.Core.Entities.Core;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

public class OrganizationMemberService(
    OrganizationMemberRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<OrganizationMember>(repository, context, mapper)
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
        CancellationToken ct = default
    )
    {
        var result = await _organizationMemberRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Organization member not found.");

        return new SuccessResult<OrganizationMemberResponseDto>(result);
    }
}
