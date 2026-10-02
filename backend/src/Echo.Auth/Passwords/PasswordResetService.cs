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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Passwords;

public class PasswordResetService(
    PasswordResetRepository passwordVerificationTokenRepository,
    UserRepository userRepository,
    JwtTokenService refreshTokenService,
    [FromKeyedServices("Resend")] IEmailService emailService,
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
    public async Task<IOperationResult> SendForgotPasswordLinkToEmail(
        string email,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.password.forgot");

        User? user;
        using (instrumentation.ActivitySource.StartActivity("svc.password.fetch.user_by_email"))
        {
            user = await userRepository.GetByEmail(email, ct);
        }

        if (user is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            RecordPasswordResetRequest("user_not_found", "unknown");
            PasswordLog.PasswordResetRequestUserNotFound(logger);
            return new OkResult("Reset link has been sent.");
        }

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

        try
        {
            var token = tokenGenerator.GenerateToken(16);
            var tokenEntity = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHashService.Hash(token),
                ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(1),
            };

            using (instrumentation.ActivitySource.StartActivity("svc.password.persist.reset_token"))
            {
                await passwordVerificationTokenRepository.CreateRecord(tokenEntity, ct);
                await unitOfWork.CommitAsync(ct);
            }

            using (instrumentation.ActivitySource.StartActivity("svc.password.persist.email_send"))
            {
                Activity.Current?.SetTag("email.provider", "resend");
                Activity.Current?.SetTag("email.type", "password_reset");
                var resetLink = linkBuilder.BuildPasswordResetLink(token);
                var emailContent = new PasswordResetEmailBody(user.Name, resetLink);
                await emailService.SendAsync(user.EmailAddress, emailContent);
            }

            span?.SetTag("auth.result", "success");
            RecordPasswordResetRequest("success", user.CongregationId.ToString());
            PasswordLog.PasswordResetRequestSucceeded(logger, user.Id, user.CongregationId);

            return new OkResult("Reset link has been sent.");
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
            span?.SetTag("auth.result", "invalid_policy");
            RecordPasswordReset("invalid_policy", "unknown");
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
            span?.SetTag("auth.result", "token_not_found");
            RecordPasswordReset("token_not_found", "unknown");
            PasswordLog.PasswordResetFailedTokenNotFound(logger);
            return new InvalidTokenResult();
        }

        span?.SetTag("congregation.id", tokenEntity.User.CongregationId);
        span?.SetTag("user.id", tokenEntity.UserId);

        if (tokenEntity.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
        {
            span?.SetTag("auth.result", "token_expired");
            RecordPasswordReset("token_expired", tokenEntity.User.CongregationId.ToString());
            PasswordLog.PasswordResetFailedTokenExpired(
                logger,
                tokenEntity.UserId,
                tokenEntity.User.CongregationId
            );
            return new InvalidTokenResult();
        }

        if (tokenEntity.UsedAt is not null)
        {
            span?.SetTag("auth.result", "token_used");
            RecordPasswordReset("token_used", tokenEntity.User.CongregationId.ToString());
            PasswordLog.PasswordResetFailedTokenUsed(
                logger,
                tokenEntity.UserId,
                tokenEntity.User.CongregationId
            );
            return new InvalidTokenResult();
        }

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.password.validate.password_hash"))
            {
                tokenEntity.User.PasswordHash = await passwordHashService.HashAsync(newPassword);
            }

            tokenEntity.UsedAt = timeProvider.GetUtcNow().UtcDateTime;

            using (instrumentation.ActivitySource.StartActivity("svc.password.persist.revoke_sessions"))
            {
                await refreshTokenService.RevokeAllActiveSessionsForUserById(
                    tokenEntity.UserId,
                    ct
                );
                await unitOfWork.CommitAsync(ct);
            }

            span?.SetTag("auth.result", "success");
            RecordPasswordReset("success", tokenEntity.User.CongregationId.ToString());
            PasswordLog.PasswordResetSucceeded(
                logger,
                tokenEntity.UserId,
                tokenEntity.User.CongregationId
            );

            return new OkResult("Password reset successfully");
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

    private void RecordPasswordResetRequest(string result, string congregationId) =>
        instrumentation.PasswordResetRequests.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );

    private void RecordPasswordReset(string result, string congregationId) =>
        instrumentation.PasswordResets.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );
}
