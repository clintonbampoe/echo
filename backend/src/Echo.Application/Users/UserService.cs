using Echo.Data;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Users;

public class UserService(
    UserRepository repository,
    IUnitOfWork unitOfWork,
    IUserMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    ApplicationInstrumentation instrumentation,
    ILogger<UserService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        CancellationToken ct = default
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.list");

        var cursor = encoder.Decode<UserCursor>(pagination.Cursor);

        List<User> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.list"))
        {
            entities = await repository.List(congregationId, cursor, pagination.PageSize + 1, ct);
        }

        var hasMore = entities.Count > pagination.PageSize;

        if (hasMore)
            entities.RemoveAt(entities.Count - 1);
        var nextCursor = encoder.Encode(BuildCursor(entities.Last()));

        var data = mapper.ToListDto(entities);
        activity?.SetTag("user.count", data.Count);

        var res = new PagedResponse<UserResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<UserResponseDto>>(res);
    }

    public async Task<IOperationResult> GetById(Guid id, Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.get_by_id");
        activity?.SetTag("user.id", id);

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.by_id"))
        {
            entity = await repository.GetById(id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        UserCreateDto dto,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.create");

        using (instrumentation.ActivitySource.StartActivity("svc.user.validate.email"))
        {
            if (await IsEmailTaken(dto.EmailAddress, ct))
            {
                UserLog.EmailTaken(logger, dto.EmailAddress);
                return new BadRequestResult("Email already exists or is invalid.");
            }
        }

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();

        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("user.id", entity.Id);
        UserLog.Created(logger, entity.Id);

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
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.update");
        activity?.SetTag("user.id", id);

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.by_id"))
        {
            entity = await repository.GetById(id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        mapper.Patch(dto, entity);
        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        UserLog.Updated(logger, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.delete");
        activity?.SetTag("user.id", id);

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.by_id"))
        {
            entity = await repository.GetById(id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.NotFound(logger, id);
            return new NotFoundResult(id.ToString());
        }

        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
        }

        UserLog.Deleted(logger, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
    {
        {
            using var activity = instrumentation.ActivitySource.StartActivity("svc.user.search");
            activity?.SetTag("user.query", name);

            List<User> entities;
            using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.search"))
            {
                entities = await repository.Search(congregationId, name, ct);
            }

            var res = mapper.ToSearchDto(entities);
            activity?.SetTag("user.count", res.Count);
            return new SuccessResult<List<UserSearchResultDto>>(res);
        }
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await repository.IsEmailAddressTaken(emailAddress, ct);
    }

    private static UserCursor BuildCursor(User last)
    {
        return new UserCursor { Name = last.Name, Id = last.Id };
    }
}
