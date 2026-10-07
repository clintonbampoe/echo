using System.Diagnostics;
using Echo.Application.Users;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.EmailVerifications;

public class EmailVerificationService(
    EmailVerificationRepository emailVerificationTokenRepository,
    UserRepository userRepository,
    [FromKeyedServices("Resend")] IEmailService emailService,
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    ITokenHasher hashService,
    LinkBuilder linkBuilder,
    TimeProvider timeProvider,
    AuthInstrumentation instrumentation,
    ILogger<EmailVerificationService> logger
)
{
    public async Task<IOperationResult> SendVerificationLinkToEmail(
        string emailAddress,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.send");

        User? user;
        using (instrumentation.ActivitySource.StartActivity("user.lookup.by_email"))
        {
            user = await userRepository.GetByEmail(emailAddress, ct);
        }

        if (user is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            RecordVerificationSent("user_not_found", "unknown");
            EmailVerificationLog.SendVerificationFailedUserNotFound(logger);
            return new SuccessResult<string>("Email has been sent if user is valid");
        }

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

        EmailVerificationToken? existingToken;
        using (
            instrumentation.ActivitySource.StartActivity("email_verification_token.lookup.by_user")
        )
        {
            existingToken = await GetActiveTokenForUser(user.Id, ct);
        }

        if (RateLimitActive(existingToken))
        {
            span?.SetTag("auth.result", "rate_limited");
            RecordVerificationSent("rate_limited", user.CongregationId.ToString());
            EmailVerificationLog.SendVerificationFailedRateLimited(
                logger,
                user.Id,
                user.CongregationId
            );
            return new SuccessResult<string>(
                "A verification email was already sent. Please check your email inbox."
            );
        }

        existingToken?.InvalidatedAt = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            var token = tokenGenerator.GenerateToken(16);
            var tokenObject = new EmailVerificationToken(user.Id)
            {
                UserId = user.Id,
                TokenHash = hashService.Hash(token),
            };

            var verificationLink = linkBuilder.BuildEmailVerificationLink(token);
            var emailContent = new EmailVerificationMessageBody(user.Name, verificationLink);

            using (instrumentation.ActivitySource.StartActivity("email_verification_token.create"))
            {
                emailVerificationTokenRepository.Create(tokenObject);
                await unitOfWork.CommitAsync(ct);
            }

            using (instrumentation.ActivitySource.StartActivity("email.send"))
            {
                Activity.Current?.SetTag("email.provider", "resend");
                Activity.Current?.SetTag("email.type", "email_verification");
                await emailService.SendAsync(user.EmailAddress, emailContent);
            }

            span?.SetTag("auth.result", "success");
            RecordVerificationSent("success", user.CongregationId.ToString());
            EmailVerificationLog.SendVerificationSucceeded(logger, user.Id, user.CongregationId);

            return new SuccessResult<string>($"Operation Completed Successfully. Token: {token}");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            EmailVerificationLog.SendVerificationFailed(logger, ex, user.Id, user.CongregationId);
            throw;
        }
    }

    public async Task<IOperationResult> VerifyEmail(string token, CancellationToken ct = default)
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.verify");

        var hashedInput = hashService.Hash(token);

        EmailVerificationToken? tokenRecord;
        using (
            instrumentation.ActivitySource.StartActivity("email_verification_token.lookup.by_hash")
        )
        {
            tokenRecord = await emailVerificationTokenRepository.GetTokenByHash(hashedInput, ct);
        }

        if (tokenRecord is null)
        {
            span?.SetTag("auth.result", "token_not_found");
            RecordVerificationCompleted("token_not_found", "unknown");
            EmailVerificationLog.VerifyEmailFailedTokenNotFound(logger);
            return new InvalidTokenResult();
        }

        span?.SetTag("congregation.id", tokenRecord.User.CongregationId);
        span?.SetTag("user.id", tokenRecord.UserId);

        if (tokenRecord.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
        {
            span?.SetTag("auth.result", "token_expired");
            RecordVerificationCompleted(
                "token_expired",
                tokenRecord.User.CongregationId.ToString()
            );
            EmailVerificationLog.VerifyEmailFailedTokenExpired(
                logger,
                tokenRecord.UserId,
                tokenRecord.User.CongregationId
            );
            return new InvalidTokenResult();
        }

        if (tokenRecord.UsedAt is not null)
        {
            span?.SetTag("auth.result", "token_used");
            RecordVerificationCompleted("token_used", tokenRecord.User.CongregationId.ToString());
            EmailVerificationLog.VerifyEmailFailedTokenUsed(
                logger,
                tokenRecord.UserId,
                tokenRecord.User.CongregationId
            );
            return new InvalidTokenResult();
        }

        if (tokenRecord.InvalidatedAt is not null)
        {
            span?.SetTag("auth.result", "token_invalidated");
            RecordVerificationCompleted(
                "token_invalidated",
                tokenRecord.User.CongregationId.ToString()
            );
            EmailVerificationLog.VerifyEmailFailedTokenInvalidated(
                logger,
                tokenRecord.UserId,
                tokenRecord.User.CongregationId
            );
            return new InvalidTokenResult();
        }

        try
        {
            using (
                instrumentation.ActivitySource.StartActivity("email_verification_token.invalidate")
            )
            {
                tokenRecord.User.EmailVerifiedAt = timeProvider.GetUtcNow().UtcDateTime;
                tokenRecord.UsedAt = timeProvider.GetUtcNow().UtcDateTime;
                await unitOfWork.CommitAsync(ct);

                span?.SetTag("auth.result", "success");
                RecordVerificationCompleted("success", tokenRecord.User.CongregationId.ToString());
                EmailVerificationLog.VerifyEmailSucceeded(
                    logger,
                    tokenRecord.UserId,
                    tokenRecord.User.CongregationId
                );
            }

            return new SuccessResult<string>("Operation Completed successfully.");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            EmailVerificationLog.VerifyEmailFailed(
                logger,
                ex,
                tokenRecord.UserId,
                tokenRecord.User.CongregationId
            );
            throw;
        }
    }

    private bool IsTokenValid(EmailVerificationToken? token)
    {
        if (token is null)
            return false;
        if (token.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
            return false;
        if (token.UsedAt is not null)
            return false;
        if (token.InvalidatedAt is not null)
            return false;
        return true;
    }

    private bool RateLimitActive(EmailVerificationToken? token)
    {
        if (!IsTokenValid(token) || token is null)
            return false;
        return token.CreatedAt > timeProvider.GetUtcNow().UtcDateTime.AddSeconds(-60);
    }

    private async Task<EmailVerificationToken?> GetActiveTokenForUser(
        Guid userId,
        CancellationToken ct
    ) => await emailVerificationTokenRepository.GetTokenByUserId(userId, ct);

    private void RecordVerificationSent(string result, string congregationId) =>
        instrumentation.EmailVerificationsSent.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );

    private void RecordVerificationCompleted(string result, string congregationId) =>
        instrumentation.EmailVerificationsCompleted.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );
}
