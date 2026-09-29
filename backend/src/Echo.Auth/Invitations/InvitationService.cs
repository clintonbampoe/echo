using System.Diagnostics;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Invitations;

public class InvitationService(
    InvitationRepository invitationTokenRepository,
    ITokenGenerator tokenGenerator,
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
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("invitation.create");

        span?.SetTag("congregation.id", congregationId);
        span?.SetTag("created_by", createdByUserId);
        span?.SetTag("allowed_role", allowedRole.ToString());

        try
        {
            var token = tokenGenerator.GenerateToken();

            var tokenEntity = new InvitationToken
            {
                CongregationId = congregationId,
                CreatedByUserId = createdByUserId,
                AllowedRole = allowedRole,
                TokenHash = hashService.Hash(token),
                ExpiresAt = timeProvider
                    .GetUtcNow()
                    .UtcDateTime.AddDays(expiryDays ?? _defaultExpiryDays),
            };

            await invitationTokenRepository.Create(tokenEntity, ct);
            await unitOfWork.CommitAsync(ct);

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
        var hashedInput = hashService.Hash(token);
        InvitationToken? tokenRecord;
        using (instrumentation.ActivitySource.StartActivity("invitation.lookup.byhash"))
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

    private void RecordInvitationCreated(string congregationId, string allowedRole) =>
        instrumentation.InvitationsCreated.Add(
            1,
            new TagList { { "congregation.id", congregationId }, { "allowed_role", allowedRole } }
        );
}
