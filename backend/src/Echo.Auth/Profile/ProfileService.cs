using Echo.Application.Users;
using Echo.Auth.Sessions;
using Echo.Data;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;

namespace Echo.Auth.Profile;

public class ProfileService(
    UserRepository userRepository,
    JwtTokenService jwtTokenService,
    IPasswordHasher passwordHasher,
    IUserMapper mapper,
    IUnitOfWork unitOfWork,
    AuthInstrumentation instrumentation
)
{
    public async Task<IOperationResult> Get(Guid congregationId, Guid userId, CancellationToken ct)
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.profile.get");

        var user = await userRepository.GetById(congregationId, userId, ct);
        if (user is null)
            return new NotFoundResult(userId.ToString());

        return new SuccessResult<UserResponseDto>(mapper.ToDto(user));
    }

    public async Task<IOperationResult> Update(
        Guid congregationId,
        Guid userId,
        ProfileUpdateDto dto,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.profile.update");

        if (Validate(dto) is { } problem)
            return new BadRequestResult(problem);

        var user = await userRepository.GetById(congregationId, userId, ct);
        if (user is null)
            return new NotFoundResult(userId.ToString());

        var changingPassword = dto.NewPassword is not null;
        if (
            changingPassword
            && !await passwordHasher.VerifyAsync(dto.CurrentPassword!, user.PasswordHash)
        )
            return new BadRequestResult("The current password is incorrect.");

        Apply(user, dto);

        if (changingPassword)
        {
            user.PasswordHash = await passwordHasher.HashAsync(dto.NewPassword!);
            // As with a password reset, changing the password signs the user out everywhere.
            await jwtTokenService.RevokeAllActiveSessionsForUserById(userId, ct);
        }

        await unitOfWork.CommitAsync(ct);

        return new SuccessResult<UserResponseDto>(mapper.ToDto(user));
    }

    // Returns what is wrong with the request, or null when it can be applied.
    internal static string? Validate(ProfileUpdateDto dto)
    {
        if (dto.FirstName is not null && string.IsNullOrWhiteSpace(dto.FirstName))
            return "firstName cannot be blank.";

        if (dto.LastName is not null && string.IsNullOrWhiteSpace(dto.LastName))
            return "lastName cannot be blank.";

        if ((dto.CurrentPassword is null) != (dto.NewPassword is null))
            return "To change the password, send both currentPassword and newPassword.";

        if (dto.NewPassword is not null && !PasswordPolicy.IsValid(dto.NewPassword, out var error))
            return error;

        return null;
    }

    // Copies the names that were sent. The role and email address are never touched here.
    internal static void Apply(User user, ProfileUpdateDto dto)
    {
        if (dto.FirstName is not null)
            user.FirstName = dto.FirstName.Trim();

        if (dto.LastName is not null)
            user.LastName = dto.LastName.Trim();

        if (dto.OtherNames is not null)
            user.OtherNames = dto.OtherNames.Trim();
    }
}
