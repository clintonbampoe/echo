using System.Diagnostics;
using Echo.Application.Congregations;
using Echo.Application.Users;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.EmailVerifications;

public class EmailVerificationService(
    EmailVerificationRepository emailVerificationTokenRepository,
    UserRepository userRepository,
    CongregationRepository congregationRepository,
    IEmailQueue emailQueue,
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    ITokenHasher hashService,
    LinkBuilder linkBuilder,
    TimeProvider timeProvider,
    AuthInstrumentation instrumentation,
    ILogger<EmailVerificationService> logger
)
{
    public async Task<IOperationResult> GetStatus(
        Guid congregationId,
        Guid userId,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.status");

        var user = await userRepository.GetById(congregationId, userId, ct);
        if (user is null)
            return new NotFoundResult(userId.ToString());

        return new SuccessResult<VerificationStatusDto>(
            new VerificationStatusDto
            {
                Verified = user.EmailVerifiedAt is not null,
                VerifiedAt = user.EmailVerifiedAt,
                EmailAddress = user.EmailAddress,
            }
        );
    }

    public async Task<IOperationResult> SendVerificationLinkToEmail(
        string emailAddress,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.send");

        User? user;
        user = await userRepository.GetByEmail(emailAddress, ct);

        if (user is null)
        {
            RecordVerificationSent(span, "user_not_found", "unknown");
            EmailVerificationLog.SendVerificationFailedUserNotFound(logger);
            return new SuccessResult<string>("Email has been sent if user is valid");
        }

        return await SendVerificationLink(user, ct);
    }

    /// <summary>
    /// Issues a verification token for a user we already have and queues the email. Does not wait
    /// for the email to be sent.
    /// </summary>
    public async Task<IOperationResult> SendVerificationLink(User user, CancellationToken ct)
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.issue");

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

        EmailVerificationToken? existingToken;
        existingToken = await GetActiveTokenForUser(user.Id, ct);

        if (RateLimitActive(existingToken))
        {
            RecordVerificationSent(span, "rate_limited", user.CongregationId.ToString());
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
                CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            };

            var verificationLink = linkBuilder.BuildEmailVerificationLink(token);
            var emailContent = new EmailVerificationMessageBody(
                user.FirstName,
                verificationLink
            );

            using (instrumentation.ActivitySource.StartActivity("email_verification_token.create"))
            {
                await emailVerificationTokenRepository.PruneTokens(user.Id, ct);
                emailVerificationTokenRepository.Create(tokenObject);
                await unitOfWork.CommitAsync(ct);
            }

            emailQueue.TryEnqueue(
                new EmailMessage(
                    user.EmailAddress,
                    emailContent,
                    "email_verification",
                    user.Id,
                    user.CongregationId
                )
            );

            RecordVerificationSent(span, "success", user.CongregationId.ToString());
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
        tokenRecord = await emailVerificationTokenRepository.GetTokenByHash(hashedInput, ct);

        if (tokenRecord is null)
        {
            RecordVerificationCompleted(span, "token_not_found", "unknown");
            EmailVerificationLog.VerifyEmailFailedTokenNotFound(logger);
            return new InvalidTokenResult();
        }

        span?.SetTag("congregation.id", tokenRecord.User.CongregationId);
        span?.SetTag("user.id", tokenRecord.UserId);

        var problem = TokenValidity.Problem(
            tokenRecord.ExpiresAt,
            tokenRecord.UsedAt,
            tokenRecord.InvalidatedAt,
            timeProvider.GetUtcNow().UtcDateTime
        );
        if (problem is not null)
        {
            var congregationId = tokenRecord.User.CongregationId;
            RecordVerificationCompleted(span, problem, congregationId.ToString());
            switch (problem)
            {
                case "token_expired":
                    EmailVerificationLog.VerifyEmailFailedTokenExpired(
                        logger,
                        tokenRecord.UserId,
                        congregationId
                    );
                    break;
                case "token_used":
                    EmailVerificationLog.VerifyEmailFailedTokenUsed(
                        logger,
                        tokenRecord.UserId,
                        congregationId
                    );
                    break;
                default:
                    EmailVerificationLog.VerifyEmailFailedTokenInvalidated(
                        logger,
                        tokenRecord.UserId,
                        congregationId
                    );
                    break;
            }
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

                RecordVerificationCompleted(
                    span,
                    "success",
                    tokenRecord.User.CongregationId.ToString()
                );
                EmailVerificationLog.VerifyEmailSucceeded(
                    logger,
                    tokenRecord.UserId,
                    tokenRecord.User.CongregationId
                );
            }

            await QueueWelcomeEmail(tokenRecord.User, ct);

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

    /// <summary>
    /// Queues the email that follows a verified account: "church ready" for the person who
    /// created the congregation, "welcome" for everyone else. The account is already verified,
    /// so a failure here is logged and never fails the request.
    /// </summary>
    private async Task QueueWelcomeEmail(User user, CancellationToken ct)
    {
        try
        {
            var congregation = await congregationRepository.GetById(user.CongregationId, ct);
            var churchName = congregation?.Name ?? "your church";

            IEmailContent content;
            string kind;
            if (await userRepository.IsFirstUserOfCongregation(user, ct))
            {
                content = new ChurchReadyEmailBody(
                    user.FirstName,
                    churchName,
                    linkBuilder.BuildInvitationsLink()
                );
                kind = "church_ready";
            }
            else
            {
                content = new WelcomeEmailBody(
                    user.FirstName,
                    churchName,
                    linkBuilder.BuildDashboardLink()
                );
                kind = "welcome";
            }

            emailQueue.TryEnqueue(
                new EmailMessage(user.EmailAddress, content, kind, user.Id, user.CongregationId)
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            EmailVerificationLog.WelcomeEmailFailed(logger, ex, user.Id, user.CongregationId);
        }
    }

    private bool IsTokenValid(EmailVerificationToken? token) =>
        token is not null
        && TokenValidity.Problem(
            token.ExpiresAt,
            token.UsedAt,
            token.InvalidatedAt,
            timeProvider.GetUtcNow().UtcDateTime
        )
            is null;

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

    private void RecordVerificationSent(Activity? span, string result, string congregationId) =>
        instrumentation.Record(
            instrumentation.EmailVerificationsSent,
            span,
            result,
            congregationId
        );

    private void RecordVerificationCompleted(
        Activity? span,
        string result,
        string congregationId
    ) =>
        instrumentation.Record(
            instrumentation.EmailVerificationsCompleted,
            span,
            result,
            congregationId
        );
}
