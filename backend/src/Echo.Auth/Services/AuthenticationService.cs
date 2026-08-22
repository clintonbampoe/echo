using Echo.Application.HttpResults;
using Echo.Application.Services.Hashing;
using Echo.Auth.Dtos;
using Echo.Auth.Models;
using Echo.Core.Repositories;
using Echo.Domain.Data;

namespace Echo.Auth.Services;

public class AuthenticationService(
    AppDbContext dbContext,
    UserRepository userRepository,
    AccessTokenGenerator accessTokenGenerator,
    RefreshTokenService refreshTokenService,
    ITokenHasher hashService
)
{
    public async Task<IOperationResult> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        var user = await userRepository.GetActiveUserByEmail(email, ct);

        if (user is null)
            return new BadRequestResult("Email or password is invalid.");

        var isPasswordValid = await hashService.VerifyAsync(password, user.PasswordHash);

        if (!isPasswordValid)
            return new BadRequestResult("Email or password is invalid.");

        if (user.EmailVerifiedAt is null)
            return new BadRequestResult("Verify your email before logging in.");

        var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(user);
        var (refreshTokenEntity, plainRefreshToken) = await refreshTokenService.IssueAsync(
            user.Id,
            ct
        );

        await dbContext.SaveChangesAsync(ct);

        var tokenPair = new TokenPairResponseDtos()
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = plainRefreshToken,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
        };

        return new SuccessResult<TokenPairResponseDtos>(tokenPair);
    }

    public async Task<IOperationResult> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        var result = await refreshTokenService.ValidateAndRotateAsync(refreshToken, ct);

        if (!result.Success)
            return new BadRequestResult(MapFailureReason(result.FailureReason!.Value));

        var user = await userRepository.GetActiveUserById(result.UserId, ct);

        if (user is null)
            return new InternalServerError();

        var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(user);

        await dbContext.SaveChangesAsync(ct);

        var tokenPair = new TokenPairResponseDtos()
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = result.NewRefreshToken!,
            RefreshTokenExpiresAt = result.NewRefreshTokenExpiresAt!.Value,
        };

        return new SuccessResult<TokenPairResponseDtos>(tokenPair);
    }

    public async Task<IOperationResult> LogoutAsync(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        await refreshTokenService.RevokeAsync(refreshToken, ct);
        await dbContext.SaveChangesAsync(ct);
        return new OkResult("Logged out.");
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
