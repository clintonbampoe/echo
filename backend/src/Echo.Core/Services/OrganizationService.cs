using AutoMapper;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Core.Services.Base;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Shared.HttpResults;
using Echo.Shared.HttpResults.Interfaces;
using Echo.Shared.Pagination;
using Echo.Shared.Query;

namespace Echo.Core.Services;

public class OrganizationService(
    OrganizationRepository repository,
    AppDbContext context,
    IMapper mapper
) : PrimaryServiceBase<Organization>(repository, context, mapper)
{
    private readonly OrganizationRepository _organizationRepository = repository;

    public override async Task<IOperationResult> GetPageAsync(
        Guid congregationId,
        PaginationParameters paginationParameters,
        QueryParameters? queryParameters,
        CancellationToken ct = default
    )
    {
        var result = await _organizationRepository.GetPageAsync(
            congregationId,
            paginationParameters,
            queryParameters,
            ct
        );
        return new SuccessResult<PagedResponse<OrganizationListResponseDto>>(result);
    }

    public override async Task<IOperationResult> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        var result = await _organizationRepository.GetByIdAsync(id, ct);

        if (result is null)
            return new NotFoundResult("Organization not found.");

        return new SuccessResult<OrganizationResponseDto>(result);
    }
}
