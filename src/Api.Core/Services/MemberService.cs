using Api.Core.Common.HttpResults;
using Api.Core.Common.HttpResults.Interfaces;
using Api.Core.Common.Pagination;
using Api.Core.Common.Query;
using Api.Core.Data;
using Api.Core.Dtos;
using Api.Core.Entities;
using Api.Core.Repositories;
using Api.Core.Services.Base;
using AutoMapper;

namespace Api.Core.Services;

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
        CancellationToken ct = default
    )
    {
        var result = await _memberRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Member not found.");

        return new SuccessResult<MemberResponseDto>(result);
    }
}
