using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Query;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.UserMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Core.Services;

public class UserService(
    UserRepository repository,
    IUnitOfWork unitOfWork,
    IUserMapper mapper,
    IIdGenerator idGenerator)
{
    public async Task<IOperationResult> GetById(
        Guid id,
        Guid congregationId,
        CancellationToken ct = default
    )
    {
        var result = await repository.GetById(id, ct);

        if (result is null)
            return new NotFoundResult("Invalid request.");

        return new SuccessResult<UserResponseDto>(result);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        UserCreateDto dto,
        CancellationToken ct = default
    )
    {
        if (await IsEmailTaken(dto.EmailAddress, ct))
            return new BadRequestResult("Email already exists or is invalid.");

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        await repository.Create(entity, ct);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Update(Guid congregationId, Guid id, UserUpdateDto dto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

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

        return new SuccessResult<PagedResponse<UserListResponseDto>>(result);
    }

    public async Task<IOperationResult> SearchUsersByName(
        Guid congregationId,
        string searchString,
        CancellationToken ct
    )
    {
        var results = await repository.SearchUsersByName(congregationId, searchString, ct);
        return new SuccessResult<List<UserListResponseDto>>(results);
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await repository.IsEmailAddressTaken(emailAddress, ct);
    }
}
