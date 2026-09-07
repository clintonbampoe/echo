using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.OrganizationMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class OrganizationService(
    OrganizationRepository repository,
    IUnitOfWork unitOfWork,
    IOrganizationMapper mapper,
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
        return new SuccessResult<PagedResponse<OrganizationListResponseDto>>(result);
    }

    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, congregationId, ct);

        if (result is null)
            return new NotFoundResult("Organization not found.");

        return new SuccessResult<OrganizationResponseDto>(result);
    }

    public async Task<IOperationResult> Create(Guid congregationId, OrganizationCreateDto dto, CancellationToken ct)
    {
        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<OrganizationResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, OrganizationUpdateDto dto,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> GetSummary(
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetSummary(congregationId, ct);
        return new SuccessResult<OrganizationSummaryDto>(result);
    }
}
