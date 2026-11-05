using System.Diagnostics;
using Echo.Application.Congregations;
using Echo.Application.Users;
using Echo.Auth.EmailVerifications;
using Echo.Auth.Invitations;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;
using Echo.Shared.Services.Generators;
using Echo.Shared.Services.Hashing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Echo.Auth.Registrations;

public class RegistrationService(
    CongregationRepository congregationRepository,
    UserRepository userRepository,
    EmailVerificationService emailVerificationService,
    IEmailQueue emailQueue,
    LinkBuilder linkBuilder,
    SessionService sessionService,
    InvitationService invitationService,
    CongregationProvisioningService provisioningService,
    IUnitOfWork unitOfWork,
    IUserMapper userMapper,
    ICongregationMapper congregationMapper,
    IPasswordHasher passwordHashService,
    IIdGenerator idGenerator,
    TimeProvider timeProvider,
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
        using var span = instrumentation.ActivitySource.StartActivity(
            "svc.registration.congregation"
        );

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
            RecordCongregationRegistration(span, "invalid_policy");
            RegistrationLog.CongregationRegistrationFailedPolicyViolation(logger);
            return new BadRequestResult(policyError!);
        }

        bool emailTaken;
        using (instrumentation.ActivitySource.StartActivity("svc.registration.fetch.user_by_email"))
        {
            emailTaken = await IsEmailTaken(user.EmailAddress, ct);
        }

        if (emailTaken)
        {
            RecordCongregationRegistration(span, "email_taken");
            RegistrationLog.CongregationRegistrationFailedEmailTaken(logger);
            return new BadRequestResult("Email already in use");
        }

        try
        {
            // The mapper copies the plain password into PasswordHash, so hash it before saving.
            user.PasswordHash = await passwordHashService.HashAsync(userDto.Password, ct);
            user.PasswordChangedAt = timeProvider.GetUtcNow().UtcDateTime;

            TokenPairResponseDtos session;
            using (
                var activity = instrumentation.ActivitySource.StartActivity(
                    "svc.registration.persist.setup"
                )
            )
            {
                congregationRepository.Create(congregation);
                activity?.AddEvent(new ActivityEvent("congregation.create"));

                userRepository.Create(user);
                activity?.AddEvent(new ActivityEvent("user.create"));

                provisioningService.ProvisionDefaults(congregation.Id);
                activity?.AddEvent(new ActivityEvent("congregation.provision_defaults"));

                session = await sessionService.StartSession(user, ct, prune: false);
                activity?.AddEvent(new ActivityEvent("session.start"));

                await unitOfWork.CommitAsync(ct);
                activity?.AddEvent(new ActivityEvent("db.commit"));
            }

            await emailVerificationService.SendVerificationLink(user, ct);
            QueueSetUpChurchEmail(user, congregation.Name);

            RecordCongregationRegistration(span, "success");
            RegistrationLog.CongregationRegistrationSucceeded(logger, congregation.Id, user.Id);

            return new SuccessResult<TokenPairResponseDtos>(session);
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
        using var span = instrumentation.ActivitySource.StartActivity("svc.registration.user");

        InvitationToken? invitation;
        using (instrumentation.ActivitySource.StartActivity("svc.registration.validate.invitation"))
        {
            invitation = await invitationService.Validate(request.Token, ct);
        }

        if (invitation is null)
        {
            RecordUserRegistration(span, "invalid_invitation", "unknown");
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
            RecordUserRegistration(span, "invalid_policy", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationFailedPolicyViolation(
                logger,
                invitation.CongregationId
            );
            return new BadRequestResult(policyError!);
        }

        bool emailTaken;
        using (instrumentation.ActivitySource.StartActivity("svc.registration.fetch.user_by_email"))
        {
            emailTaken = await IsEmailTaken(request.UserInfo.EmailAddress, ct);
        }

        if (emailTaken)
        {
            RecordUserRegistration(span, "email_taken", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationFailedEmailTaken(logger, invitation.CongregationId);
            return new BadRequestResult("Email already in use");
        }

        try
        {
            string passwordHash;
            using (
                instrumentation.ActivitySource.StartActivity(
                    "svc.registration.validate.password_hash"
                )
            )
            {
                passwordHash = await passwordHashService.HashAsync(request.UserInfo.Password, ct);
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
                PasswordChangedAt = timeProvider.GetUtcNow().UtcDateTime,
            };

            span?.SetTag("user.id", user.Id);

            // Take one of the invitation's uses in the same transaction as the user, so a limited
            // invitation cannot be over-used by people registering at the same moment, and a
            // signup that fails gives the use back.
            var claimed = false;
            TokenPairResponseDtos? session = null;
            try
            {
                using (
                    instrumentation.ActivitySource.StartActivity("svc.registration.persist.user")
                )
                {
                    await unitOfWork.ExecuteInTransactionAsync(
                        async token =>
                        {
                            claimed = await invitationService.TryClaimUse(invitation.Id, token);
                            if (!claimed)
                                return;

                            userRepository.Create(user);
                            session = await sessionService.StartSession(user, token, prune: false);
                            await unitOfWork.CommitAsync(token);
                        },
                        ct
                    );
                }
            }
            catch (DbUpdateException ex) when (IsDuplicateEmail(ex))
            {
                // Someone registered this address between the check above and the insert.
                RecordUserRegistration(span, "email_taken", invitation.CongregationId.ToString());
                RegistrationLog.UserRegistrationFailedEmailTaken(logger, invitation.CongregationId);
                return new BadRequestResult("Email already in use");
            }

            if (!claimed)
            {
                RecordUserRegistration(span, "invalid_invitation", invitation.CongregationId.ToString());
                RegistrationLog.UserRegistrationFailedInvalidInvitation(logger);
                return new BadRequestResult("Invitation is invalid, expired, or revoked.");
            }

            using (
                instrumentation.ActivitySource.StartActivity(
                    "svc.registration.queue.email_verification"
                )
            )
            {
                await emailVerificationService.SendVerificationLink(user, ct);
            }

            await QueueWelcomeEmail(user, ct);

            RecordUserRegistration(span, "success", invitation.CongregationId.ToString());
            RegistrationLog.UserRegistrationSucceeded(logger, user.Id, invitation.CongregationId);

            return new SuccessResult<TokenPairResponseDtos>(session!);
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RegistrationLog.UserRegistrationFailed(logger, ex, invitation.CongregationId);
            throw;
        }
    }

    // The two emails below follow the verification email. The account already exists by now, so
    // a failure is logged and never fails the sign-up.
    private void QueueSetUpChurchEmail(User admin, string congregationName)
    {
        try
        {
            var content = new SetUpChurchEmailBody(
                admin.FirstName,
                congregationName,
                linkBuilder.BuildDashboardLink()
            );
            emailQueue.TryEnqueue(
                new EmailMessage(
                    admin.EmailAddress,
                    content,
                    "set_up_church",
                    admin.Id,
                    admin.CongregationId
                )
            );
        }
        catch (Exception ex)
        {
            RegistrationLog.SetUpChurchEmailFailed(logger, ex, admin.Id, admin.CongregationId);
        }
    }

    private async Task QueueWelcomeEmail(User user, CancellationToken ct)
    {
        try
        {
            var congregation = await congregationRepository.GetById(user.CongregationId, ct);
            var content = new WelcomeEmailBody(
                user.FirstName,
                congregation?.Name ?? "your church",
                user.Role,
                linkBuilder.BuildDashboardLink()
            );
            emailQueue.TryEnqueue(
                new EmailMessage(user.EmailAddress, content, "welcome", user.Id, user.CongregationId)
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RegistrationLog.WelcomeEmailFailed(logger, ex, user.Id, user.CongregationId);
        }
    }

    // 23505 is Postgres' unique_violation; users has a unique index on the email address.
    private static bool IsDuplicateEmail(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private async Task<bool> IsEmailTaken(string emailAddress, CancellationToken ct) =>
        await userRepository.IsEmailAddressTaken(emailAddress, ct);

    private void RecordCongregationRegistration(Activity? span, string result) =>
        instrumentation.Record(instrumentation.CongregationRegistrations, span, result);

    private void RecordUserRegistration(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.UserRegistrations, span, result, congregationId);
}
