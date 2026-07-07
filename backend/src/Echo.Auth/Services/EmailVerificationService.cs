using Echo.Application.HttpResults;
using Echo.Application.Services;
using Echo.Application.Services.Email;
using Echo.Application.Services.Hashing;
using Echo.Auth.Models;
using Echo.Auth.Repositories;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OkResult = Echo.Application.HttpResults.OkResult;

namespace Echo.Auth.Services;

public class EmailVerificationService(
    AppDbContext dbContext,
    EmailVerificationTokenRepository emailVerificationTokenRepository,
    UserRepository userRepository,
    [FromKeyedServices("Resend")] IEmailService emailService,
    ITokenGenerator tokenGenerator,
    [FromKeyedServices("Sha256")] IHashService hashService,
    AuthLinkBuilder linkBuilder)
{

    public async Task<IOperationResult> SendVerificationLinkToEmail(Guid userId, CancellationToken ct = default)
    {
        var token = tokenGenerator.GenerateToken(16);

        var tokenObject = new EmailVerificationToken(userId)
        {
            UserId = userId,
            TokenHash = await hashService.HashPasswordAsync(token)
        };

        var recordCreatedSuccessfully = await emailVerificationTokenRepository.CreateRecord(tokenObject, ct);
        await dbContext.SaveChangesAsync(ct);

        if (!recordCreatedSuccessfully)
            return new InternalServerError();

        var userInfo = await userRepository.GetByIdAsync(userId, ct);
        if (userInfo == null)
            return new InternalServerError();

        var verificationLink = linkBuilder.BuildEmailVerificationLink(token);

        var emailContent = new VerifyEmailContent(userInfo.Name, verificationLink);

        await emailService.SendAsync(userInfo.EmailAddress, emailContent);

        return new OkResult("Operation Completed Successfully.");
    }

    public Task<ActionResult> VerifyEmail(Guid userId, string token)
    {
        throw new NotImplementedException();
    }
}
