using Echo.Application.HttpResults;
using Echo.Application.Pagination;
using Echo.Application.Services.Encoders;
using Echo.Application.Services.Generators;
using Echo.Core.Dtos;
using Echo.Core.Mapping.UserMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;

namespace Echo.Core.Services;

public class UserService(
    UserRepository repository,
    IUnitOfWork unitOfWork,
    IEncoder encoder,
    IUserMapper mapper,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        var cursor = encoder.Decode<UserCursor>(pagination.Cursor);

        var entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);

        var hasMore = entities.Count > pagination.PageSize;

        if (hasMore)
            entities.RemoveAt(entities.Count - 1);
        var nextCursor = encoder.Encode(BuildCursor(entities.Last()));

        var data = mapper.ToListDto(entities);
        var res = new PagedResponse<UserResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<UserResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        var entity = await repository.GetById(id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        UserCreateDto dto,
        CancellationToken ct
    )
    {
        if (await IsEmailTaken(dto.EmailAddress, ct))
            return new BadRequestResult("Email already exists or is invalid.");

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        repository.Create(entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new CreatedAtResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        UserUpdateDto dto,
        CancellationToken ct
    )
    {
        var entity = await repository.GetById(id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        mapper.Patch(dto, entity);
        await unitOfWork.CommitAsync(ct);

        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        var entity = await repository.GetById(id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        repository.SoftDelete(entity);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        var entities = await repository.Search(congregationId, name, ct);
        var res = mapper.ToSearchDto(entities);
        return new SuccessResult<List<UserSearchResultDto>>(res);
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await repository.IsEmailAddressTaken(emailAddress, ct);
    }

    private UserCursor BuildCursor(User last)
    {
        return new UserCursor { Name = last.Name, Id = last.Id };
    }
}
