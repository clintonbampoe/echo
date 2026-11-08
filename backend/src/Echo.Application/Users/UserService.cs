using Echo.Data;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Users;

public class UserService(
    UserRepository repository,
    IUnitOfWork unitOfWork,
    IUserMapper mapper,
    IEncoder encoder,
    IIdGenerator idGenerator,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider,
    ApplicationInstrumentation instrumentation,
    LinkGenerator linker,
    ILogger<UserService> logger
)
{
    public async Task<IOperationResult> List(
        Guid congregationId,
        PaginationRequest pagination,
        UserFilter filter,
        CancellationToken ct = default
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.list");

        var cursor = encoder.Decode<UserCursor>(pagination.Cursor);

        List<User> entities;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.list"))
        {
            entities = await repository.List(congregationId, filter, cursor, pagination.PageSize + 1, ct);
        }

        var hasMore = entities.Count > pagination.PageSize;

        string? nextCursor = null;
        if (hasMore)
        {
            entities.RemoveAt(entities.Count - 1);
            nextCursor = encoder.Encode(BuildCursor(entities.Last()));
        }

        var data = mapper.ToListDto(entities);
        activity?.SetTag("user.count", data.Count);
        UserLog.Listed(logger, congregationId, data.Count);

        var res = new PagedResponse<UserResponseDto>(hasMore, nextCursor, data);
        return new SuccessResult<PagedResponse<UserResponseDto>>(res);
    }

    public async Task<IOperationResult> Summary(Guid congregationId, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.summary");

        var counts = await repository.CountByRole(congregationId, ct);

        // Every role gets an entry, so a role nobody holds yet still has its card.
        var roles = Enum.GetValues<UserRole>()
            .Select(r => counts.FirstOrDefault(c => c.Role == r) ?? new UserRoleCountDto { Role = r })
            .ToList();

        var res = new UserSummaryDto
        {
            Total = roles.Sum(r => r.Count),
            Disabled = roles.Sum(r => r.Disabled),
            Roles = roles,
        };
        return new SuccessResult<UserSummaryDto>(res);
    }

    public async Task<IOperationResult> GetById(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.get_by_id");
        activity?.SetTag("user.id", id);

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.NotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        UserLog.Found(logger, congregationId, id);
        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Create(
        Guid congregationId,
        UserCreateDto dto,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.create");

        using (instrumentation.ActivitySource.StartActivity("svc.user.validate.email"))
        {
            if (await IsEmailTaken(dto.EmailAddress, ct))
            {
                UserLog.CreateEmailTaken(logger, congregationId);
                return new ConflictResult("Email already exists or is invalid.");
            }
        }

        var entity = mapper.ToEntity(dto);
        entity.CongregationId = congregationId;
        entity.Id = idGenerator.Generate();
        entity.PasswordHash = await passwordHasher.HashAsync(dto.Password);
        entity.PasswordChangedAt = timeProvider.GetUtcNow().UtcDateTime;

        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            repository.Create(entity);
            await unitOfWork.CommitAsync(ct);
        }

        activity?.SetTag("user.id", entity.Id);
        UserLog.Created(logger, congregationId, entity.Id);

        var res = mapper.ToDto(entity);

        var location =
            linker.GetPathByName(httpContext, "GetUserById", new { id = res.Id })
            ?? throw new InvalidOperationException("Route 'GetUserById' is not registered.");
        return new CreatedResult<UserResponseDto>(location, res);
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid actorId,
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
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.UpdateNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        if (dto.Role.HasValue && dto.Role != entity.Role)
        {
            var refusal = await CheckCanLoseAdminPower(congregationId, actorId, entity, ct);
            if (refusal is not null)
                return refusal;
        }

        mapper.Patch(dto, entity);
        if (dto.Password is not null)
        {
            entity.PasswordHash = await passwordHasher.HashAsync(dto.Password);
            entity.PasswordChangedAt = timeProvider.GetUtcNow().UtcDateTime;
        }

        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            await unitOfWork.CommitAsync(ct);
        }

        UserLog.Updated(logger, congregationId, id);

        var res = mapper.ToDto(entity);
        return new SuccessResult<UserResponseDto>(res);
    }

    public async Task<IOperationResult> Delete(
        Guid congregationId,
        Guid actorId,
        Guid id,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.delete");
        activity?.SetTag("user.id", id);

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.user.fetch.by_id"))
        {
            entity = await repository.GetById(congregationId, id, ct);
        }

        if (entity is null)
        {
            activity?.SetTag("user.found", false);
            UserLog.DeleteNotFound(logger, congregationId, id);
            return new NotFoundResult(id.ToString());
        }

        var refusal = await CheckCanLoseAdminPower(congregationId, actorId, entity, ct);
        if (refusal is not null)
            return refusal;

        using (instrumentation.ActivitySource.StartActivity("svc.user.persist"))
        {
            repository.SoftDelete(entity);
            await unitOfWork.CommitAsync(ct);
            await repository.RevokeSessions(entity.Id, timeProvider.GetUtcNow().UtcDateTime, ct);
        }

        UserLog.Deleted(logger, congregationId, id);

        return new NoContentResult();
    }

    public async Task<IOperationResult> Disable(
        Guid congregationId,
        Guid actorId,
        Guid id,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.disable");
        activity?.SetTag("user.id", id);

        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        // Disabling twice is not an error; the first time stays on record.
        if (entity.DisabledAt is not null)
            return new SuccessResult<UserResponseDto>(mapper.ToDto(entity));

        var refusal = await CheckCanLoseAdminPower(congregationId, actorId, entity, ct);
        if (refusal is not null)
            return refusal;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        entity.DisabledAt = now;
        await unitOfWork.CommitAsync(ct);
        await repository.RevokeSessions(entity.Id, now, ct);

        UserLog.Disabled(logger, congregationId, id, actorId);
        return new SuccessResult<UserResponseDto>(mapper.ToDto(entity));
    }

    public async Task<IOperationResult> Enable(
        Guid congregationId,
        Guid actorId,
        Guid id,
        CancellationToken ct
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity("svc.user.enable");
        activity?.SetTag("user.id", id);

        var entity = await repository.GetById(congregationId, id, ct);
        if (entity is null)
            return new NotFoundResult(id.ToString());

        if (entity.DisabledAt is not null)
        {
            entity.DisabledAt = null;
            await unitOfWork.CommitAsync(ct);
            UserLog.Enabled(logger, congregationId, id, actorId);
        }

        return new SuccessResult<UserResponseDto>(mapper.ToDto(entity));
    }

    public async Task<IOperationResult> Search(
        Guid congregationId,
        string name,
        CancellationToken ct
    )
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
        UserLog.Searched(logger, congregationId, name, res.Count);
        return new SuccessResult<List<UserSearchResultDto>>(res);
    }

    /// <summary>
    /// Refuses a change that would take away the caller's own access, or the last admin who can sign in. Without
    /// the second rule a church could lock itself out with no way back except the database.
    /// </summary>
    private async Task<IOperationResult?> CheckCanLoseAdminPower(
        Guid congregationId,
        Guid actorId,
        User target,
        CancellationToken ct
    )
    {
        if (target.Id == actorId)
        {
            UserLog.Refused(logger, "self", congregationId, target.Id);
            return new ConflictResult(
                "You cannot change your own account. Ask another admin.",
                "CANNOT_MODIFY_SELF"
            );
        }

        var losesAdmin = target.Role == UserRole.Admin && target.DisabledAt is null;
        if (losesAdmin && !await repository.HasOtherActiveAdmin(congregationId, target.Id, ct))
        {
            UserLog.Refused(logger, "last_admin", congregationId, target.Id);
            return new ConflictResult(
                "This is the only admin who can sign in. Make someone else an admin first.",
                "LAST_ADMIN"
            );
        }

        return null;
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
