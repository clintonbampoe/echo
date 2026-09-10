using Echo.Application.HttpResults;
using Echo.Application.Services.Email;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Echo.Auth.Models;
using Echo.Auth.Repositories;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class EmailVerificationService(
    EmailVerificationTokenRepository emailVerificationTokenRepository,
    UserRepository userRepository,
    [FromKeyedServices("Resend")] IEmailService emailService,
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    ITokenHasher hashService,
    AuthLinkBuilder linkBuilder,
    TimeProvider timeProvider
)
{
    public async Task<IOperationResult> SendVerificationLinkToEmail(
        string emailAddress,
        CancellationToken ct
    )
    {
        var user = await userRepository.GetByEmail(emailAddress, ct);

        if (user is null)
            return new GenericEmailSentSuccessResult();

        var existingToken = await GetActiveTokenForUser(user.Id, ct);

        if (RateLimitActive(existingToken))
            return new OkResult(
                "A verification email was already sent. Please check your email inbox."
            );

        if (existingToken is not null)
            existingToken.InvalidatedAt = timeProvider.GetUtcNow().UtcDateTime;

        var token = tokenGenerator.GenerateToken(16);
        var tokenObject = new EmailVerificationToken(user.Id)
        {
            UserId = user.Id,
            TokenHash = hashService.Hash(token),
        };

        emailVerificationTokenRepository.Create(tokenObject);
        var userInfo = await userRepository.GetById(user.Id, ct);
        if (userInfo == null)
            return new InternalServerError();

        var verificationLink = linkBuilder.BuildEmailVerificationLink(token);
        var emailContent = new VerifyEmailContent(userInfo.Name, verificationLink);

        await unitOfWork.CommitAsync(ct);
        await emailService.SendAsync(userInfo.EmailAddress, emailContent);

        // TODO: Remove token in production
        // replace with:
        // return new GenericEmailSentSuccessResult();
        return new OkResult($"Operation Completed Successfully. Token: {token}");
    }

    public async Task<IOperationResult> VerifyEmail(string token, CancellationToken ct = default)
    {
        var hashedInput = hashService.Hash(token);
        var tokenRecord = await emailVerificationTokenRepository.GetTokenByHash(hashedInput, ct);

        if (tokenRecord is null)
            return new InvalidTokenResult();

        if (!IsTokenValid(tokenRecord))
            return new InvalidTokenResult();

        var user = tokenRecord.User;
        user.EmailVerifiedAt = timeProvider.GetUtcNow().UtcDateTime;
        tokenRecord.UsedAt = timeProvider.GetUtcNow().UtcDateTime;

        await unitOfWork.CommitAsync(ct);
        return new OkResult("Operation Completed successfully.");
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
        if (IsTokenValid(token))
        {
            if (token is null)
                return false;

            var isActive = token.CreatedAt > timeProvider.GetUtcNow().UtcDateTime.AddSeconds(-60);
            return isActive;
        }

        return false;
    }

    private async Task<EmailVerificationToken?> GetActiveTokenForUser(
        Guid userId,
        CancellationToken ct
    )
    {
        return await emailVerificationTokenRepository.GetTokenByUserId(userId, ct);
    }
}
