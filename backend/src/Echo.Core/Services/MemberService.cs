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

public class MemberService(MemberRepository repository, AppDbContext context, IMapper mapper)
    : PrimaryServiceBase<Member>(repository, context, mapper)
{
    private readonly MemberRepository _memberRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _memberRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<MemberListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _memberRepository.GetByIdAsync(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Member not found.");

        return new SuccessResult<MemberResponseDto>(result);
    }

    public async Task<IOperationResult> GetSummaryAsync(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await _memberRepository.GetSummaryAsync(congregationId, ct);
        return new SuccessResult<MemberSummaryDto>(result);
    }

    public async Task<IOperationResult> SearchMembersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await _memberRepository.SearchMembersByName(congregationId, searchString, ct);
        return new SuccessResult<List<MemberListResponseDto>>(results);
    }
}
