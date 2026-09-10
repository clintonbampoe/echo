using Echo.Application.HttpResults;
using Echo.Application.Services.Email;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Echo.Auth.Models;
using Echo.Auth.Repositories;
using Echo.Auth.Validation;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class PasswordResetService(
    PasswordVerificationTokenRepository passwordVerificationTokenRepository,
    UserRepository userRepository,
    RefreshTokenService refreshTokenService,
    [FromKeyedServices("Resend")] IEmailService emailService,
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    ITokenHasher tokenHashService,
    IPasswordHasher passwordHashService,
    AuthLinkBuilder linkBuilder,
    TimeProvider timeProvider
)
{
    public async Task<IOperationResult> SendForgotPasswordLinkToEmail(
        string email,
        CancellationToken ct
    )
    {
        var user = await userRepository.GetByEmail(email, ct);
        if (user is null)
            return new OkResult("Reset link has been sent.");

        var token = tokenGenerator.GenerateToken(16);
        var tokenEntity = new PasswordVerificationToken
        {
            UserId = user.Id,
            TokenHash = tokenHashService.Hash(token),
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(1),
        };

        await passwordVerificationTokenRepository.CreateRecord(tokenEntity, ct);
        await unitOfWork.CommitAsync(ct);

        var resetLink = linkBuilder.BuildPasswordResetLink(token);
        var emailContent = new ResetPasswordContent(user.Name, resetLink);
        await emailService.SendAsync(user.EmailAddress, emailContent);

        return new OkResult("Reset link has been sent.");
    }

    public async Task<IOperationResult> ResetPassword(
        string token,
        string newPassword,
        CancellationToken ct
    )
    {
        var hashedInput = tokenHashService.Hash(token);
        var tokenEntity = await passwordVerificationTokenRepository.GetTokenRecordByHashWithUser(
            hashedInput,
            ct
        );

        var passwordIsValid = PasswordPolicy.IsValid(newPassword, out var policyError);
        if (!passwordIsValid)
            return new BadRequestResult(policyError!);

        if (tokenEntity is null)
            return new InvalidTokenResult();

        if (
            tokenEntity.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime
            || tokenEntity.UsedAt is not null
        )
            return new InvalidTokenResult();

        tokenEntity.User.PasswordHash = await passwordHashService.HashAsync(newPassword);
        tokenEntity.UsedAt = timeProvider.GetUtcNow().UtcDateTime;

        await refreshTokenService.RevokeAllActiveSessionsForUserByUserId(tokenEntity.UserId, ct);

        await unitOfWork.CommitAsync(ct);
        return new OkResult("Password reset successfully");
    }
}
