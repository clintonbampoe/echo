using System.Diagnostics;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Invitations;

public class InvitationService(
    InvitationRepository invitationTokenRepository,
    IEncoder encoder,
    IUnitOfWork unitOfWork,
    ITokenHasher hashService,
    TimeProvider timeProvider,
    AuthInstrumentation instrumentation,
    ILogger<InvitationService> logger
)
{
    private const int _defaultExpiryDays = 30;

    public async Task<IOperationResult> CreateInvitationToken(
        Guid congregationId,
        Guid createdByUserId,
        UserRole allowedRole,
        int? expiryDays,
        string? label,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.create");

        span?.SetTag("congregation.id", congregationId);
        span?.SetTag("created_by", createdByUserId);
        span?.SetTag("allowed_role", allowedRole.ToString());

        try
        {
            var token = InvitationCode.Generate();

            var tokenEntity = new InvitationToken
            {
                CongregationId = congregationId,
                CreatedByUserId = createdByUserId,
                AllowedRole = allowedRole,
                TokenHash = hashService.Hash(InvitationCode.Normalize(token)!),
                Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
                ExpiresAt = timeProvider
                    .GetUtcNow()
                    .UtcDateTime.AddDays(expiryDays ?? _defaultExpiryDays),
            };

            using (instrumentation.ActivitySource.StartActivity("svc.invitation.persist.token"))
            {
                await invitationTokenRepository.Create(tokenEntity, ct);
                await unitOfWork.CommitAsync(ct);
            }

            span?.SetTag("auth.result", "success");
            RecordInvitationCreated(congregationId.ToString(), allowedRole.ToString());
            InvitationLog.InvitationCreateSucceeded(
                logger,
                congregationId,
                createdByUserId,
                allowedRole
            );

            return new SuccessResult<InviteResponseDto>(
                new InviteResponseDto
                {
                    Id = tokenEntity.Id,
                    Label = tokenEntity.Label,
                    Token = token,
                    AllowedRole = tokenEntity.AllowedRole,
                    ExpiresAt = tokenEntity.ExpiresAt,
                }
            );
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            InvitationLog.InvitationCreateFailed(logger, ex, congregationId, createdByUserId);
            throw;
        }
    }

    public async Task<InvitationToken?> Validate(string token, CancellationToken ct = default)
    {
        // Codes are typed by hand, so case, dashes and look-alikes are forgiven before hashing.
        var normalized = InvitationCode.Normalize(token);
        if (normalized is null)
        {
            InvitationLog.InvitationValidationFailedNotFound(logger);
            return null;
        }

        var hashedInput = hashService.Hash(normalized);
        InvitationToken? tokenRecord;
        using (instrumentation.ActivitySource.StartActivity("svc.invitation.fetch.token_by_hash"))
        {
            tokenRecord = await invitationTokenRepository.GetTokenRecordByHash(hashedInput, ct);
        }

        if (tokenRecord is null)
        {
            InvitationLog.InvitationValidationFailedNotFound(logger);
            return null;
        }

        if (tokenRecord.IsRevoked)
        {
            InvitationLog.InvitationValidationFailedRevoked(logger, tokenRecord.CongregationId);
            return null;
        }

        if (tokenRecord.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
        {
            InvitationLog.InvitationValidationFailedExpired(logger, tokenRecord.CongregationId);
            return null;
        }

        return tokenRecord;
    }

    public async Task<IOperationResult> List(
        Guid congregationId,
        InvitationFilters filters,
        PaginationRequest pagination,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.list");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var cursor = encoder.Decode<InvitationCursor>(pagination.Cursor);

        var entities = await invitationTokenRepository.List(
            congregationId,
            filters.Status,
            cursor,
            pagination.PageSize + 1,
            now,
            ct
        );

        var hasMore = entities.Count > pagination.PageSize;
        if (hasMore)
            entities.RemoveAt(entities.Count - 1);

        var next = hasMore
            ? encoder.Encode(new InvitationCursor { CreatedAt = entities[^1].CreatedAt, Id = entities[^1].Id })
            : null;

        var data = entities.Select(e => ToDto(e, now)).ToList();
        return new SuccessResult<PagedResponse<InvitationResponseDto>>(
            new PagedResponse<InvitationResponseDto>(hasMore, next, data)
        );
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid id,
        InvitationUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.update");

        var token = await invitationTokenRepository.GetById(congregationId, id, ct);
        if (token is null)
            return new NotFoundResult(id.ToString());

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (ApplyUpdate(token, dto, now) is { } problem)
            return new BadRequestResult(problem);

        await unitOfWork.CommitAsync(ct);
        return new SuccessResult<InvitationResponseDto>(ToDto(token, now));
    }

    public async Task<IOperationResult> Revoke(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.revoke");

        var token = await invitationTokenRepository.GetById(congregationId, id, ct);
        if (token is null)
            return new NotFoundResult(id.ToString());

        ApplyRevoke(token);
        await unitOfWork.CommitAsync(ct);

        InvitationLog.InvitationRevoked(logger, congregationId, id);
        return new SuccessResult<InvitationResponseDto>(
            ToDto(token, timeProvider.GetUtcNow().UtcDateTime)
        );
    }

    public async Task<IOperationResult> Delete(Guid congregationId, Guid id, CancellationToken ct)
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.delete");

        var token = await invitationTokenRepository.GetById(congregationId, id, ct);
        if (token is null)
            return new NotFoundResult(id.ToString());

        invitationTokenRepository.SoftDelete(token);
        await unitOfWork.CommitAsync(ct);

        return new NoContentResult();
    }

    /// <summary>
    /// What the join screen shows before the person creates an account. A bad, expired, revoked or
    /// deleted code all give the same 404, so the endpoint reveals nothing about which codes exist.
    /// </summary>
    public async Task<IOperationResult> Lookup(string code, CancellationToken ct)
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.lookup");

        var normalized = InvitationCode.Normalize(code);
        if (normalized is null)
            return new NotFoundResult("invitation");

        var token = await invitationTokenRepository.GetTokenRecordByHashWithCongregation(
            hashService.Hash(normalized),
            ct
        );
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (token is null || token.StatusAt(now) != InvitationStatus.Active)
            return new NotFoundResult("invitation");

        return new SuccessResult<InvitationLookupDto>(
            new InvitationLookupDto
            {
                CongregationName = token.Congregation.Name,
                City = token.Congregation.City,
                Region = token.Congregation.Region,
                AllowedRole = token.AllowedRole,
                ExpiresAt = token.ExpiresAt,
            }
        );
    }

    // The count is informational, and it runs after the account exists, so a failure here is logged
    // and swallowed rather than turning a successful registration into an error.
    public async Task RecordUse(Guid invitationId, CancellationToken ct)
    {
        try
        {
            await invitationTokenRepository.RecordUse(invitationId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            InvitationLog.InvitationUseNotRecorded(logger, ex, invitationId);
        }
    }

    // Returns what is wrong with the request, or null when it was applied. Nothing changes on an error.
    internal static string? ApplyUpdate(InvitationToken token, InvitationUpdateDto dto, DateTime now)
    {
        if (dto.ExpiryDays is not null && token.IsRevoked)
            return "A revoked invitation cannot be extended. Create a new one.";

        if (dto.Label is not null)
            token.Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim();

        if (dto.ExpiryDays is not null)
            token.ExpiresAt = now.AddDays(dto.ExpiryDays.Value);

        return null;
    }

    internal static void ApplyRevoke(InvitationToken token) => token.IsRevoked = true;

    private static InvitationResponseDto ToDto(InvitationToken token, DateTime now) =>
        new()
        {
            Id = token.Id,
            Label = token.Label,
            AllowedRole = token.AllowedRole,
            CreatedByName = token.CreatedBy.Name,
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt,
            Status = token.StatusAt(now),
            UseCount = token.UseCount,
        };

    private void RecordInvitationCreated(string congregationId, string allowedRole) =>
        instrumentation.InvitationsCreated.Add(
            1,
            new TagList { { "congregation.id", congregationId }, { "allowed_role", allowedRole } }
        );
}
