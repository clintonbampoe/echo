using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Application.Services.Email;
using Echo.Application.Services.Hashing;
using Echo.Auth.Models;
using Echo.Auth.Repositories;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Echo.Domain.Entities.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class EmailVerificationService(
    AppDbContext dbContext,
    EmailVerificationTokenRepository emailVerificationTokenRepository,
    UserRepository userRepository,
    [FromKeyedServices("Resend")] IEmailService emailService,
    ITokenGenerator tokenGenerator,
    [FromKeyedServices("Sha256")] IHashService hashService,
    AuthLinkBuilder linkBuilder
)
{
    public async Task<IOperationResult> SendVerificationLinkToEmail(
        string emailAddress,
        CancellationToken ct = default
    )
    {
        var user = await userRepository.GetActiveUserByEmail(emailAddress, ct);

        if (user is null)
            return new OkResult("Operation completed successfully.");

        var existingToken = await GetActiveTokenForUser(user.Id, ct);

        if (RateLimitActive(existingToken))
            return new OkResult("A verification email was already sent. Please check your email inbox.");

        if (existingToken is not null)
            existingToken.InvalidatedAt = DateTime.UtcNow;

        var token = tokenGenerator.GenerateToken(16);

        var tokenObject = new EmailVerificationToken(user.Id)
        {
            UserId = user.Id,
            TokenHash = await hashService.HashPasswordAsync(token),
        };

        var recordCreatedSuccessfully = await emailVerificationTokenRepository.CreateRecord(
            tokenObject,
            ct
        );

        if (!recordCreatedSuccessfully)
            return new InternalServerError();

        var userInfo = await userRepository.GetByIdAsync(user.Id, ct);
        if (userInfo == null)
            return new InternalServerError();

        var verificationLink = linkBuilder.BuildEmailVerificationLink(token);

        var emailContent = new VerifyEmailContent(userInfo.Name, verificationLink);

        await dbContext.SaveChangesAsync(ct);

        await emailService.SendAsync(userInfo.EmailAddress, emailContent);

        // TODO: Remove token in production
        return new OkResult($"Operation Completed Successfully. Token: {token}");
    }

    public async Task<IOperationResult> VerifyEmail(string token, CancellationToken ct = default)
    {
        var hashedInput = await hashService.HashPasswordAsync(token);

        var tokenRecord = await emailVerificationTokenRepository.GetTokenRecordByHashWithUser(
            hashedInput,
            ct
        );

        if (tokenRecord is null)
            return new NotFoundResult("Not found.");

        if (!IsTokenValid(tokenRecord))
            return new BadRequestResult("Token is invalid or already used");

        var user = tokenRecord.User;
        user.EmailVerifiedAt = DateTime.UtcNow;
        tokenRecord.UsedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return new OkResult("Operation Completed successfully.");
    }

    private bool IsTokenValid(EmailVerificationToken? token)
    {
        if (token is null)
            return false;

        if (token.ExpiresAt <= DateTime.UtcNow)
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

            var isActive = token.CreatedAt > DateTime.UtcNow.AddSeconds(-60);
            return isActive;
        }

        return false;
    }

    private async Task<EmailVerificationToken?> GetActiveTokenForUser(Guid userId, CancellationToken ct)
    {
        return await emailVerificationTokenRepository.GetActiveTokenForUser(userId, ct);
    }
}
