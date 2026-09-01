using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Application.Services.Email;
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
    AppDbContext dbContext,
    PasswordVerificationTokenRepository passwordVerificationTokenRepository,
    UserRepository userRepository,
    RefreshTokenService refreshTokenService,
    [FromKeyedServices("Resend")] IEmailService emailService,
    ITokenGenerator tokenGenerator,
    ITokenHasher tokenHashService,
    IPasswordHasher passwordHashService,
    AuthLinkBuilder linkBuilder
)
{
    public async Task<IOperationResult> ForgotPasswordAsync(
        string email,
        CancellationToken ct = default
    )
    {
        var user = await userRepository.GetActiveUserByEmail(email, ct);

        if (user is null)
            return new OkResult("Reset link has been sent.");

        var token = tokenGenerator.GenerateToken(16);

        var tokenEntity = new PasswordVerificationToken
        {
            UserId = user.Id,
            TokenHash = await tokenHashService.HashAsync(token),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        await passwordVerificationTokenRepository.CreateRecord(tokenEntity, ct);
        await dbContext.SaveChangesAsync(ct);

        var resetLink = linkBuilder.BuildPasswordResetLink(token);
        var emailContent = new ResetPasswordContent(user.Name, resetLink);
        await emailService.SendAsync(user.EmailAddress, emailContent);

        return new OkResult("Reset link has been sent.");
    }

    public async Task<IOperationResult> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken ct = default
    )
    {
        var hashedInput = await tokenHashService.HashAsync(token);
        var tokenEntity = await passwordVerificationTokenRepository.GetTokenRecordByHashWithUser(
            hashedInput,
            ct
        );

        var passwordIsValid = PasswordPolicy.IsValid(newPassword, out var policyError);
        if (!passwordIsValid)
            return new BadRequestResult(policyError!);

        if (tokenEntity is null)
            return new InvalidTokenResult();

        if (tokenEntity.ExpiresAt <= DateTime.UtcNow || tokenEntity.UsedAt is not null)
            return new InvalidTokenResult();

        tokenEntity.User.PasswordHash = await passwordHashService.HashAsync(newPassword);
        tokenEntity.UsedAt = DateTime.UtcNow;

        await refreshTokenService.RevokeAllActiveSessionsForUser(tokenEntity.UserId, ct);

        await dbContext.SaveChangesAsync(ct);

        return new OkResult("Password reset successfully");
    }
}
