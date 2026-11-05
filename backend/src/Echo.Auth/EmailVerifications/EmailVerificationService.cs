using System.Diagnostics;
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
    private const string SentMessage = "Email has been sent if user is valid";

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

        // The user and their live link come back in one query.
        var found = await emailVerificationTokenRepository.GetUserWithActiveToken(
            emailAddress,
            ct
        );

        if (found is not { } match)
        {
            RecordVerificationSent(span, "user_not_found", "unknown");
            EmailVerificationLog.SendVerificationFailedUserNotFound(logger);
            return new SuccessResult<string>(SentMessage);
        }

        return await IssueLink(match.User, match.ActiveToken, ct);
    }

    /// <summary>
    /// Issues a verification token for a user we already have and queues the email. Does not wait
    /// for the email to be sent.
    /// </summary>
    public async Task<IOperationResult> SendVerificationLink(User user, CancellationToken ct) =>
        await IssueLink(user, await GetActiveTokenForUser(user.Id, ct), ct);

    private async Task<IOperationResult> IssueLink(
        User user,
        EmailVerificationToken? existingToken,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.issue");

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

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
            var tokenObject = NewToken(user, token);

            using (instrumentation.ActivitySource.StartActivity("email_verification_token.create"))
            {
                await emailVerificationTokenRepository.PruneTokens(user.Id, ct);
                emailVerificationTokenRepository.Create(tokenObject);
                await unitOfWork.CommitAsync(ct);
            }

            QueueVerificationEmail(user, token);

            // The token is only ever sent by email. Answering with it would let anyone who knows an
            // address verify that account, and a different answer for a known address would tell
            // callers which addresses have accounts, so this matches the unknown-address reply.
            return new SuccessResult<string>(SentMessage);
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            EmailVerificationLog.SendVerificationFailed(logger, ex, user.Id, user.CongregationId);
            throw;
        }
    }

    /// <summary>
    /// Adds a verification token for a brand-new user to the context, without saving, so a signup
    /// stores the user, the session and the token in one commit. A new user has no old tokens to
    /// look up or clear. Once the commit succeeds, call <see cref="QueueVerificationEmail"/> with
    /// the returned token.
    /// </summary>
    public string StageFirstToken(User user)
    {
        var token = tokenGenerator.GenerateToken(16);
        emailVerificationTokenRepository.Create(NewToken(user, token));
        return token;
    }

    /// <summary>Queues the verification email for a token that is already saved. Does not wait for it to be sent.</summary>
    public void QueueVerificationEmail(User user, string token)
    {
        using var span = instrumentation.ActivitySource.StartActivity("email.verification.queue");

        span?.SetTag("congregation.id", user.CongregationId);
        span?.SetTag("user.id", user.Id);

        emailQueue.TryEnqueue(
            new EmailMessage(
                user.EmailAddress,
                new EmailVerificationMessageBody(
                    user.FirstName,
                    linkBuilder.BuildEmailVerificationLink(token)
                ),
                "email_verification",
                user.Id,
                user.CongregationId
            )
        );

        RecordVerificationSent(span, "success", user.CongregationId.ToString());
        EmailVerificationLog.SendVerificationSucceeded(logger, user.Id, user.CongregationId);
    }

    private EmailVerificationToken NewToken(User user, string token) =>
        new(user.Id)
        {
            UserId = user.Id,
            TokenHash = hashService.Hash(token),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
        };

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
