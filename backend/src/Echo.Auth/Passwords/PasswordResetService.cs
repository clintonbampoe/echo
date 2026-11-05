using System.Diagnostics;
using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Passwords;

public class PasswordResetService(
    PasswordResetRepository passwordVerificationTokenRepository,
    UserRepository userRepository,
    JwtTokenService refreshTokenService,
    IEmailQueue emailQueue,
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    ITokenHasher tokenHashService,
    IPasswordHasher passwordHashService,
    LinkBuilder linkBuilder,
    TimeProvider timeProvider,
    AuthInstrumentation instrumentation,
    ILogger<PasswordResetService> logger
)
{
    private const int ResendCooldownSeconds = 60;

    public async Task<IOperationResult> SendForgotPasswordLinkToEmail(
        string email,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.password.forgot");

        User? user;
        user = await userRepository.GetByEmail(email, ct);

        if (user is null)
        {
            RecordPasswordResetRequest(span, "user_not_found", "unknown");
            PasswordLog.PasswordResetRequestUserNotFound(logger);
            return new SuccessResult<string>("Reset link has been sent.");
        }

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

        try
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;

            List<PasswordResetToken> activeTokens;
            using (instrumentation.ActivitySource.StartActivity("svc.password.fetch.active_tokens"))
            {
                activeTokens = await passwordVerificationTokenRepository.GetActiveTokensForUser(
                    user.Id,
                    ct
                );
            }

            // One link a minute is enough; answering the same way keeps this from telling anyone
            // whether the address has an account.
            if (activeTokens.Any(t => t.CreatedAt > now.AddSeconds(-ResendCooldownSeconds)))
            {
                RecordPasswordResetRequest(span, "rate_limited", user.CongregationId.ToString());
                PasswordLog.PasswordResetRequestRateLimited(logger, user.Id, user.CongregationId);
                return new SuccessResult<string>("Reset link has been sent.");
            }

            // Only the newest link works.
            foreach (var old in activeTokens)
                old.InvalidatedAt = now;

            var token = tokenGenerator.GenerateToken(16);
            var tokenEntity = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHashService.Hash(token),
                ExpiresAt = now.AddHours(1),
                CreatedAt = now,
            };

            using (instrumentation.ActivitySource.StartActivity("svc.password.persist.reset_token"))
            {
                await passwordVerificationTokenRepository.PruneTokens(user.Id, ct);
                await passwordVerificationTokenRepository.CreateRecord(tokenEntity, ct);
                await unitOfWork.CommitAsync(ct);
            }

            var resetLink = linkBuilder.BuildPasswordResetLink(token);
            emailQueue.TryEnqueue(
                new EmailMessage(
                    user.EmailAddress,
                    new PasswordResetEmailBody(user.FirstName, resetLink),
                    "password_reset",
                    user.Id,
                    user.CongregationId
                )
            );

            RecordPasswordResetRequest(span, "success", user.CongregationId.ToString());
            PasswordLog.PasswordResetRequestSucceeded(logger, user.Id, user.CongregationId);

            return new SuccessResult<string>("Reset link has been sent.");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            PasswordLog.PasswordResetRequestFailed(logger, ex, user.Id, user.CongregationId);
            throw;
        }
    }

    public async Task<IOperationResult> ResetPassword(
        string token,
        string newPassword,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.password.reset");

        var passwordIsValid = PasswordPolicy.IsValid(newPassword, out var policyError);
        if (!passwordIsValid)
        {
            RecordPasswordReset(span, "invalid_policy", "unknown");
            PasswordLog.PasswordResetFailedPolicyViolation(logger);
            return new BadRequestResult(policyError!);
        }

        var hashedInput = tokenHashService.Hash(token);

        PasswordResetToken? tokenEntity;
        using (instrumentation.ActivitySource.StartActivity("svc.password.fetch.token_by_hash"))
        {
            tokenEntity = await passwordVerificationTokenRepository.GetTokenRecordByHashWithUser(
                hashedInput,
                ct
            );
        }

        if (tokenEntity is null)
        {
            RecordPasswordReset(span, "token_not_found", "unknown");
            PasswordLog.PasswordResetFailedTokenNotFound(logger);
            return new InvalidTokenResult();
        }

        span?.SetTag("congregation.id", tokenEntity.User.CongregationId);
        span?.SetTag("user.id", tokenEntity.UserId);

        var problem = TokenValidity.Problem(
            tokenEntity.ExpiresAt,
            tokenEntity.UsedAt,
            tokenEntity.InvalidatedAt,
            timeProvider.GetUtcNow().UtcDateTime
        );
        if (problem is not null)
        {
            var congregationId = tokenEntity.User.CongregationId;
            RecordPasswordReset(span, problem, congregationId.ToString());
            switch (problem)
            {
                case "token_expired":
                    PasswordLog.PasswordResetFailedTokenExpired(
                        logger,
                        tokenEntity.UserId,
                        congregationId
                    );
                    break;
                case "token_used":
                    PasswordLog.PasswordResetFailedTokenUsed(
                        logger,
                        tokenEntity.UserId,
                        congregationId
                    );
                    break;
                default:
                    PasswordLog.PasswordResetFailedTokenInvalidated(
                        logger,
                        tokenEntity.UserId,
                        congregationId
                    );
                    break;
            }
            return new InvalidTokenResult();
        }

        try
        {
            using (
                instrumentation.ActivitySource.StartActivity("svc.password.validate.password_hash")
            )
            {
                tokenEntity.User.PasswordHash = await passwordHashService.HashAsync(newPassword);
            }

            var changedAt = timeProvider.GetUtcNow().UtcDateTime;
            tokenEntity.User.PasswordChangedAt = changedAt;
            tokenEntity.UsedAt = changedAt;

            using (
                instrumentation.ActivitySource.StartActivity("svc.password.persist.revoke_sessions")
            )
            {
                // The bulk revoke runs at once, so it shares a transaction with the save: the
                // new password and the sign-out of every session both happen, or neither does.
                await unitOfWork.ExecuteInTransactionAsync(
                    async token =>
                    {
                        await refreshTokenService.RevokeAllActiveSessionsForUserById(
                            tokenEntity.UserId,
                            token
                        );
                        await unitOfWork.CommitAsync(token);
                    },
                    ct
                );
            }

            emailQueue.TryEnqueue(
                new EmailMessage(
                    tokenEntity.User.EmailAddress,
                    new PasswordChangedEmailBody(
                        tokenEntity.User.FirstName,
                        tokenEntity.User.EmailAddress,
                        changedAt,
                        linkBuilder.BuildForgotPasswordLink()
                    ),
                    "password_changed",
                    tokenEntity.UserId,
                    tokenEntity.User.CongregationId
                )
            );

            RecordPasswordReset(span, "success", tokenEntity.User.CongregationId.ToString());
            PasswordLog.PasswordResetSucceeded(
                logger,
                tokenEntity.UserId,
                tokenEntity.User.CongregationId
            );

            return new SuccessResult<string>("Password reset successfully");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            PasswordLog.PasswordResetFailed(
                logger,
                ex,
                tokenEntity.UserId,
                tokenEntity.User.CongregationId
            );
            throw;
        }
    }

    private void RecordPasswordResetRequest(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.PasswordResetRequests, span, result, congregationId);

    private void RecordPasswordReset(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.PasswordResets, span, result, congregationId);
}
