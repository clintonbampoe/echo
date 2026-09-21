using Echo.Application.HttpResults;
using Echo.Application.Services.Hashing;
using Echo.Core.Users;
using Echo.Data;

namespace Echo.Auth.Sessions;

public class SessionService(
    UserRepository userRepository,
    JwtTokenGenerator accessTokenGenerator,
    JwtTokenService jwtTokenService,
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
        var (refreshTokenEntity, plainRefreshToken) = await jwtTokenService.IssueToken(
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

    public async Task<IOperationResult> RefreshAccessToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        var result = await jwtTokenService.ValidateAndRotateAsync(refreshToken, ct);
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

    public async Task<IOperationResult> RevokeAccessToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        await jwtTokenService.RevokeToken(refreshToken, ct);
        await unitOfWork.CommitAsync(ct);
        return new OkResult("Token revoked successfully.");
    }

    private static string MapFailureReason(RefreshSessionFailure reason) =>
        reason switch
        {
            RefreshSessionFailure.NotFound => "Invalid session. Please log in again.",
            RefreshSessionFailure.Expired => "Your session has expired. Please log in again.",
            RefreshSessionFailure.Reused =>
                "Your session was invalidated for security reasons. Please log in again.",
            RefreshSessionFailure.UserInactive => "This account is no longer active.",
            _ => "Please log in again.",
        };
}
