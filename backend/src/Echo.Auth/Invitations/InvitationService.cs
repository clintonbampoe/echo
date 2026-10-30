using System.Diagnostics;
using Echo.Application.Caching;
using Echo.Application.Congregations;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Pagination;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Encoders;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Invitations;

public class InvitationService(
    InvitationRepository invitationTokenRepository,
    CongregationRepository congregationRepository,
    InvitationCodeProtector codeProtector,
    SummaryCache summaries,
    IEmailService emailService,
    LinkBuilder linkBuilder,
    IEncoder encoder,
    IUnitOfWork unitOfWork,
    ITokenHasher hashService,
    TimeProvider timeProvider,
    AuthInstrumentation instrumentation,
    ILogger<InvitationService> logger
)
{
    public const int DefaultExpiryDays = 30;
    public const int MaxExpiryDays = 365;
    public const int MaxUsesLimit = 10_000;
    public const int ExpiringSoonDays = 7;

    public async Task<IOperationResult> CreateInvitationToken(
        Guid congregationId,
        Guid createdByUserId,
        InviteRequest request,
        CancellationToken ct = default
    )
    {
        if (ExpiryProblem(request.ExpiryDays, request.NeverExpires) is { } problem)
            return new BadRequestResult(problem);

        var (entity, code) = await Create(
            congregationId,
            createdByUserId,
            request.AllowedRole,
            request.ExpiryDays,
            request.NeverExpires,
            request.MaxUses,
            request.Label,
            ct
        );
        return new SuccessResult<InviteResponseDto>(ToCreatedDto(entity, code, emailedTo: null));
    }

    /// <summary>
    /// Creates a single-use invitation and emails it. If the email cannot be sent the invitation
    /// still exists, and the response has no <c>EmailedTo</c>, so the admin can copy the code instead.
    /// </summary>
    public async Task<IOperationResult> CreateAndEmail(
        Guid congregationId,
        Guid createdByUserId,
        InviteByEmailRequest request,
        CancellationToken ct = default
    )
    {
        if (ExpiryProblem(request.ExpiryDays, request.NeverExpires) is { } problem)
            return new BadRequestResult(problem);

        var emailAddress = request.EmailAddress.Trim();
        var (entity, code) = await Create(
            congregationId,
            createdByUserId,
            request.AllowedRole,
            request.ExpiryDays,
            request.NeverExpires,
            maxUses: 1,
            string.IsNullOrWhiteSpace(request.Label) ? emailAddress : request.Label,
            ct
        );

        string? emailedTo = null;
        try
        {
            var congregation = await congregationRepository.GetById(congregationId, ct);
            var content = new InvitationEmailBody(
                congregation?.Name ?? "your congregation",
                entity.AllowedRole,
                code,
                linkBuilder.BuildJoinLink(code),
                entity.ExpiresAt
            );
            using (instrumentation.ActivitySource.StartActivity("email.send"))
            {
                Activity.Current?.SetTag("email.provider", "resend");
                Activity.Current?.SetTag("email.type", "invitation");
                await emailService.SendAsync(emailAddress, content);
            }

            emailedTo = emailAddress;
            InvitationLog.InvitationEmailSent(logger, congregationId, entity.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            InvitationLog.InvitationEmailFailed(logger, ex, congregationId, entity.Id);
        }

        return new SuccessResult<InviteResponseDto>(ToCreatedDto(entity, code, emailedTo));
    }

    private async Task<(InvitationToken Entity, string Code)> Create(
        Guid congregationId,
        Guid createdByUserId,
        UserRole allowedRole,
        int? expiryDays,
        bool neverExpires,
        int? maxUses,
        string? label,
        CancellationToken ct
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
                EncryptedCode = codeProtector.Protect(InvitationCode.Normalize(token)!),
                Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
                ExpiresAt = ExpiryFor(timeProvider.GetUtcNow().UtcDateTime, expiryDays, neverExpires),
                MaxUses = maxUses,
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

            return (tokenEntity, token);
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

        if (tokenRecord.MaxUses <= tokenRecord.UseCount)
        {
            InvitationLog.InvitationValidationFailedExhausted(logger, tokenRecord.CongregationId);
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

    public async Task<IOperationResult> GetById(
        Guid congregationId,
        Guid id,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.invitation.get_by_id");

        var token = await invitationTokenRepository.GetById(congregationId, id, ct);
        return token is null
            ? new NotFoundResult(id.ToString())
            : new SuccessResult<InvitationResponseDto>(
                ToDto(token, timeProvider.GetUtcNow().UtcDateTime)
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
        return ToLookupResult(token, timeProvider.GetUtcNow().UtcDateTime);
    }

    // Every reason a code cannot be used gives the same answer, so a caller learns nothing about
    // which codes exist.
    internal static IOperationResult ToLookupResult(InvitationToken? token, DateTime now)
    {
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

    /// <summary>
    /// Counts one registration against the invitation. False when the last use was taken by someone
    /// else a moment ago, or it was revoked or expired meanwhile; the caller must then not create
    /// the account.
    /// </summary>
    public Task<bool> TryClaimUse(Guid invitationId, CancellationToken ct) =>
        invitationTokenRepository.TryClaimUse(invitationId, timeProvider.GetUtcNow().UtcDateTime, ct);

    public async Task<IOperationResult> GetSummary(Guid congregationId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await summaries.Serve(
            "invitations",
            congregationId,
            new { },
            token =>
                invitationTokenRepository.GetSummary(
                    congregationId,
                    now,
                    now.AddDays(ExpiringSoonDays),
                    token
                ),
            ct
        );
    }

    // Returns what is wrong with the request, or null when it was applied. Nothing changes on an error.
    internal static string? ApplyUpdate(InvitationToken token, InvitationUpdateDto dto, DateTime now)
    {
        var extending = dto.ExpiryDays is not null || dto.NeverExpires == true || dto.MaxUses is not null;
        if (extending && token.IsRevoked)
            return "A revoked invitation cannot be extended. Create a new one.";

        if (ExpiryProblem(dto.ExpiryDays, dto.NeverExpires == true) is { } problem)
            return problem;

        if (dto.MaxUses < token.UseCount)
            return $"{token.UseCount} people have already joined with this invitation, so the limit cannot be lower than that.";

        if (dto.Label is not null)
            token.Label = string.IsNullOrWhiteSpace(dto.Label) ? null : dto.Label.Trim();

        if (dto.NeverExpires == true)
            token.ExpiresAt = null;
        else if (dto.ExpiryDays is not null)
            token.ExpiresAt = now.AddDays(dto.ExpiryDays.Value);

        if (dto.MaxUses is not null)
            token.MaxUses = dto.MaxUses;

        return null;
    }

    // The expiry for a new invitation: none, or the given number of days (30 when not given).
    internal static DateTime? ExpiryFor(DateTime now, int? expiryDays, bool neverExpires) =>
        neverExpires ? null : now.AddDays(expiryDays ?? DefaultExpiryDays);

    // An invitation cannot both never expire and expire in some days.
    internal static string? ExpiryProblem(int? expiryDays, bool neverExpires) =>
        neverExpires && expiryDays is not null
            ? "Send either expiryDays or neverExpires, not both."
            : null;

    internal static void ApplyRevoke(InvitationToken token) => token.IsRevoked = true;

    private InvitationResponseDto ToDto(InvitationToken token, DateTime now) =>
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
            MaxUses = token.MaxUses,
            Code = codeProtector.Unprotect(token.EncryptedCode) is { } plain
                ? InvitationCode.Format(plain)
                : null,
        };

    private static InviteResponseDto ToCreatedDto(
        InvitationToken token,
        string code,
        string? emailedTo
    ) =>
        new()
        {
            Id = token.Id,
            Label = token.Label,
            Token = code,
            AllowedRole = token.AllowedRole,
            ExpiresAt = token.ExpiresAt,
            MaxUses = token.MaxUses,
            EmailedTo = emailedTo,
        };

    private void RecordInvitationCreated(string congregationId, string allowedRole) =>
        instrumentation.InvitationsCreated.Add(
            1,
            new TagList { { "congregation.id", congregationId }, { "allowed_role", allowedRole } }
        );
}
