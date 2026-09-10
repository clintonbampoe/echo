using Echo.Application.HttpResults;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Models;
using Echo.Core.Mapping.UserMapping;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Auth.Services;

public class AuthenticationService(
    UserRepository userRepository,
    AccessTokenGenerator accessTokenGenerator,
    RefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    IUserMapper mapper,
    IPasswordHasher hashService
)
{
    public async Task<IOperationResult> Login(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        var entity = await userRepository.GetByEmail(email, ct);
        if (entity is null)
            return new BadRequestResult("Email or password is invalid.");

        var isPasswordValid = await hashService.VerifyAsync(password, entity.PasswordHash);

        if (!isPasswordValid)
            return new BadRequestResult("Email or password is invalid.");

        if (entity.EmailVerifiedAt is null)
            return new BadRequestResult("Verify your email before logging in.");

        var user = mapper.ToAuthDto(entity);
        var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(user);
        var (refreshTokenEntity, plainRefreshToken) = await refreshTokenService.IssueToken(
            entity.Id,
            ct
        );

        await unitOfWork.CommitAsync(ct);
        var tokenPair = new TokenPairResponseDtos()
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = plainRefreshToken,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
        };
        return new SuccessResult<TokenPairResponseDtos>(tokenPair);
    }

    public async Task<IOperationResult> RefreshAuthToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        var result = await refreshTokenService.ValidateAndRotateAsync(refreshToken, ct);
        if (!result.Success)
            return new BadRequestResult(MapFailureReason(result.FailureReason!.Value));

        var entity = await userRepository.GetById(result.UserId, ct);
        if (entity is null)
            return new InternalServerError();

        var userDto = mapper.ToAuthDto(entity);
        var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(userDto);
        await unitOfWork.CommitAsync(ct);

        var tokenPair = new TokenPairResponseDtos()
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = result.NewRefreshToken!,
            RefreshTokenExpiresAt = result.NewRefreshTokenExpiresAt!.Value,
        };
        return new SuccessResult<TokenPairResponseDtos>(tokenPair);
    }

    public async Task<IOperationResult> RevokeAuthToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        await refreshTokenService.RevokeToken(refreshToken, ct);
        await unitOfWork.CommitAsync(ct);
        return new OkResult("Token revoked successfully.");
    }

    private static string MapFailureReason(RefreshTokenFailureReason reason) =>
        reason switch
        {
            RefreshTokenFailureReason.NotFound => "Invalid session. Please log in again.",
            RefreshTokenFailureReason.Expired => "Your session has expired. Please log in again.",
            RefreshTokenFailureReason.Reused =>
                "Your session was invalidated for security reasons. Please log in again.",
            RefreshTokenFailureReason.UserInactive => "This account is no longer active.",
            _ => "Please log in again.",
        };
}
