using AutoMapper;
using Echo.Application.HttpResults;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Core.Dtos;
using Echo.Core.Repositories;
using Echo.Domain.Data;
using Echo.Domain.Entities.Core;
using Echo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.Auth.Services;

public class RegistrationService(
    AppDbContext context,
    CongregationRepository congregationRepository,
    UserRepository userRepository,
    EmailVerificationService emailVerificationService,
    InvitationService invitationService,
    IMapper mapper,
    [FromKeyedServices("Bcrypt")] IHashService hashService
)
{
    public async Task<IOperationResult> RegisterCongregation(
        CongregationCreateDto congregationDto,
        UserCreateDto userDto,
        CancellationToken ct
    )
    {
        var congregation = mapper.Map<Congregation>(congregationDto);
        var user = mapper.Map<User>(userDto);
        user.Role = UserRole.Admin;
        user.CongregationId = congregation.Id;

        if (await IsEmailTaken(user.EmailAddress, ct))
            return new BadRequestResult("Email already in use");

        await HashPassword(user, userDto);

        await congregationRepository.CreateRecord(congregation, ct);
        await userRepository.CreateRecord(user, ct);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new InternalServerError();
        }

        return new OkResult("Operation completed successfully.");
    }

    public async Task<IOperationResult> RegisterMemberAsync(
        RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        var invitation = await invitationService.ValidateAsync(request.Token, ct);
        if (invitation is null)
            return new BadRequestResult("Invitation is invalid, expired, or revoked.");

        if (await IsEmailTaken(request.Email, ct))
            return new BadRequestResult("Email already in use");

        var user = new User
        {
            Name = request.Name,
            EmailAddress = request.Email,
            Role = invitation.AllowedRole,
            CongregationId = invitation.CongregationId,
            PasswordHash = await hashService.HashPasswordAsync(request.Password),
        };

        await userRepository.CreateRecord(user, ct);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return new InternalServerError();
        }

        await emailVerificationService.SendVerificationLinkToEmail(user.EmailAddress, ct);

        return new OkResult("Check your email to verify your account and complete registration.");
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await userRepository.IsEmailAddressTaken(emailAddress, ct);
    }

    private async Task HashPassword(User user, UserCreateDto userDto)
    {
        user.PasswordHash = await hashService.HashPasswordAsync(userDto.Password);
    }
}
