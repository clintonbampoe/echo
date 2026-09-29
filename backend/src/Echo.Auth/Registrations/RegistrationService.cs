using System.Diagnostics;
using Echo.Application.Congregations;
using Echo.Application.Users;
using Echo.Auth.EmailVerifications;
using Echo.Auth.Invitations;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

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
    IIdGenerator idGenerator,
    AuthInstrumentation instrumentation,
    ILogger<RegistrationService> logger
)
{
    public async Task<IOperationResult> RegisterCongregation(
        CongregationCreateDto congregationDto,
        UserCreateDto userDto,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("registration.congregation");

        var congregation = congregationMapper.ToEntity(congregationDto);
        congregation.Id = idGenerator.Generate();

        var user = userMapper.ToEntity(userDto);
        user.Id = idGenerator.Generate();
        user.Role = UserRole.Admin;
        user.CongregationId = congregation.Id;

        span?.SetTag("congregation.id", congregation.Id);
        span?.SetTag("user.id", user.Id);

        var passwordIsValid = PasswordPolicy.IsValid(userDto.Password, out var policyError);
        if (!passwordIsValid)
        {
            span?.SetTag("auth.result", "invalid_policy");
            RecordCongregationRegistration("invalid_policy");
            RegistrationLog.CongregationRegistrationFailedPolicyViolation(logger);
            return new BadRequestResult(policyError!);
        }

        bool emailTaken;
        using (instrumentation.ActivitySource.StartActivity("user.lookup.by_email"))
        {
            emailTaken = await IsEmailTaken(user.EmailAddress, ct);
        }

        if (emailTaken)
        {
            span?.SetTag("auth.result", "email_taken");
            RecordCongregationRegistration("email_taken");
            RegistrationLog.CongregationRegistrationFailedEmailTaken(logger);
            return new BadRequestResult("Email already in use");
        }

        try
        {
            using (instrumentation.ActivitySource.StartActivity("password.hash"))
            {
                user.PasswordHash = await passwordHashService.HashAsync(userDto.Password);
            }

            using (
                var activity = instrumentation.ActivitySource.StartActivity("congregation.setup")
            )
            {
                congregationRepository.Create(congregation);
                activity?.AddEvent(new ActivityEvent("congregation.create"));

                userRepository.Create(user);
                activity?.AddEvent(new ActivityEvent("user.create"));

                await unitOfWork.CommitAsync(ct);
                activity?.AddEvent(new ActivityEvent("db.commit"));
            }

            span?.SetTag("auth.result", "success");
            RecordCongregationRegistration("success");
            RegistrationLog.CongregationRegistrationSucceeded(logger, congregation.Id, user.Id);

            return new OkResult("Operation completed successfully.");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RegistrationLog.CongregationRegistrationFailed(logger, ex, congregation.Id, user.Id);
            throw;
        }
    }

    public async Task<IOperationResult> RegisterUser(
        RegisterMemberRequest request,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("registration.user");

        InvitationToken? invitation;
        using (instrumentation.ActivitySource.StartActivity("invitation.validate"))
        {
            invitation = await invitationService.Validate(request.Token, ct);
        }

        if (invitation is null)
        {
            span?.SetTag("auth.result", "invalid_invitation");
            RecordUserRegistration("invalid_invitation", "unknown");
            RegistrationLog.UserRegistrationFailedInvalidInvitation(logger);
            return new BadRequestResult("Invitation is invalid, expired, or revoked.");
        }

        span?.SetTag("congregation.id", invitation.CongregationId);

        var passwordIsValid = PasswordPolicy.IsValid(
            request.UserInfo.Password,
            out var policyError
        );
        if (!passwordIsValid)
        {
            span?.SetTag("auth.result", "invalid_policy");
            RecordUserRegistration("invalid_policy", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationFailedPolicyViolation(
                logger,
                invitation.CongregationId
            );
            return new BadRequestResult(policyError!);
        }

        bool emailTaken;
        using (instrumentation.ActivitySource.StartActivity("user.lookup.by_email"))
        {
            emailTaken = await IsEmailTaken(request.UserInfo.EmailAddress, ct);
        }

        if (emailTaken)
        {
            span?.SetTag("auth.result", "email_taken");
            RecordUserRegistration("email_taken", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationFailedEmailTaken(logger, invitation.CongregationId);
            return new BadRequestResult("Email already in use");
        }

        try
        {
            string passwordHash;
            using (instrumentation.ActivitySource.StartActivity("password.hash"))
            {
                passwordHash = await passwordHashService.HashAsync(request.UserInfo.Password);
            }

            var user = new User
            {
                Id = idGenerator.Generate(),
                LastName = request.UserInfo.LastName,
                FirstName = request.UserInfo.FirstName,
                OtherNames = request.UserInfo.OtherNames,
                EmailAddress = request.UserInfo.EmailAddress,
                Role = invitation.AllowedRole,
                CongregationId = invitation.CongregationId,
                PasswordHash = passwordHash,
            };

            span?.SetTag("user.id", user.Id);

            using (instrumentation.ActivitySource.StartActivity("user.persist"))
            {
                userRepository.Create(user);
                await unitOfWork.CommitAsync(ct);
            }

            using (instrumentation.ActivitySource.StartActivity("email.send"))
            {
                Activity.Current?.SetTag("email.provider", "resend");
                Activity.Current?.SetTag("email.type", "email_verification");
                await emailVerificationService.SendVerificationLinkToEmail(user.EmailAddress, ct);
            }

            span?.SetTag("auth.result", "success");
            RecordUserRegistration("success", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationSucceeded(logger, user.Id, invitation.CongregationId);

            return new OkResult(
                "Check your email to verify your account and complete registration."
            );
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RegistrationLog.UserRegistrationFailed(logger, ex, invitation.CongregationId);
            throw;
        }
    }

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct) =>
        await userRepository.IsEmailAddressTaken(emailAddress, ct);

    private void RecordCongregationRegistration(string result) =>
        instrumentation.CongregationRegistrations.Add(1, new TagList { { "auth.result", result } });

    private void RecordUserRegistration(string result, string congregationId) =>
        instrumentation.UserRegistrations.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );
}
