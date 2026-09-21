using Echo.Application.HttpResults;
using Echo.Application.Services.Generators;
using Echo.Application.Services.Hashing;
using Echo.Auth.EmailVerifications;
using Echo.Auth.Invitations;
using Echo.Core.Congregations;
using Echo.Core.Users;
using Echo.Data;
using Echo.Domain.Users;

namespace Echo.Auth.Registrations;

public class RegistrationService(
    CongregationRepository congregationRepository,
    UserRepository userRepository,
    EmailVerificationService emailVerificationService,
    InvitationService invitationService,
    IUnitOfWork unitOfWork,
    IUserMapper userMapper,
    ICongregationMapper congregationMapper,
    IPasswordHasher passwordHashService,
    IIdGenerator idGenerator
)
{
    public async Task<IOperationResult> RegisterCongregation(
        CongregationCreateDto congregationDto,
        UserCreateDto userDto,
        CancellationToken ct
    )
    {
        var congregation = congregationMapper.ToEntity(congregationDto);
        congregation.Id = idGenerator.Generate();

        var user = userMapper.ToEntity(userDto);
        user.Id = idGenerator.Generate();
        user.Role = UserRole.Admin;
        user.CongregationId = congregation.Id;

        var passwordIsValid = PasswordPolicy.IsValid(userDto.Password, out var policyError);
        if (!passwordIsValid)
            return new BadRequestResult(policyError!);

        if (await IsEmailTaken(user.EmailAddress, ct))
            return new BadRequestResult("Email already in use");

        await HashPassword(user, userDto);

        congregationRepository.Create(congregation);
        userRepository.Create(user);

        await unitOfWork.CommitAsync(ct);
        return new OkResult("Operation completed successfully.");
    }

    public async Task<IOperationResult> RegisterUser(
        RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        var invitation = await invitationService.Validate(request.Token, ct);
        if (invitation is null)
            return new BadRequestResult("Invitation is invalid, expired, or revoked.");

        var passwordIsValid = PasswordPolicy.IsValid(
            request.UserInfo.Password,
            out var policyError
        );

        if (!passwordIsValid)
            return new BadRequestResult(policyError!);

        if (await IsEmailTaken(request.UserInfo.EmailAddress, ct))
            return new BadRequestResult("Email already in use");

        var user = new User
        {
            LastName = request.UserInfo.LastName,
            FirstName = request.UserInfo.FirstName,
            OtherNames = request.UserInfo.OtherNames,
            EmailAddress = request.UserInfo.EmailAddress,
            Role = invitation.AllowedRole,
            CongregationId = invitation.CongregationId,
            PasswordHash = await passwordHashService.HashAsync(request.UserInfo.Password),
        };

        userRepository.Create(user);
        await unitOfWork.CommitAsync(ct);

        await emailVerificationService.SendVerificationLinkToEmail(user.EmailAddress, ct);
        return new OkResult("Check your email to verify your account and complete registration.");
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct)
    {
        return await userRepository.IsEmailAddressTaken(emailAddress, ct);
    }

    private async Task HashPassword(User user, UserCreateDto userDto)
    {
        user.PasswordHash = await passwordHashService.HashAsync(userDto.Password);
    }
}
