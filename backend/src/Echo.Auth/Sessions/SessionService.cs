using System.Diagnostics;
using Echo.Application.Users;
using Echo.Data;
using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;
using Microsoft.Extensions.Logging;

namespace Echo.Auth.Sessions;

public class SessionService(
    UserRepository userRepository,
    JwtTokenGenerator accessTokenGenerator,
    JwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    IUserMapper mapper,
    IPasswordHasher hashService,
    AuthInstrumentation instrumentation,
    ILogger<SessionService> logger
)
{
    public async Task<IOperationResult> Login(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.login");

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.user_by_email"))
        {
            entity = await userRepository.GetByEmail(email, ct);
        }

        if (entity is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            RecordLogin("user_not_found", "unknown");
            SessionLog.LoginUserNotFound(logger);
            return new BadRequestResult("Email or password is invalid.");
        }

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        using (instrumentation.ActivitySource.StartActivity("svc.session.validate.password"))
        {
            var isPasswordValid = await hashService.VerifyAsync(password, entity.PasswordHash);
            if (!isPasswordValid)
            {
                span?.SetTag("auth.result", "invalid_credentials");
                RecordLogin("invalid_credentials", entity.Id.ToString());
                SessionLog.LoginInvalidCredentials(logger, entity.Id);
                return new BadRequestResult("Email or password is invalid.");
            }
        }

        if (entity.EmailVerifiedAt is null)
        {
            span?.SetTag("auth.result", "email_not_verified");
            RecordLogin("email_not_verified", entity.Id.ToString());
            SessionLog.LoginEmailNotVerified(logger, entity.Id);
            return new UserNotVerifiedResult();
        }

        var user = mapper.ToAuthDto(entity);

        try
        {
            var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(user);

            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.tokens"))
            {
                var (refreshTokenEntity, plainRefreshToken) = await jwtTokenService.IssueToken(
                    user.Id,
                    ct
                );
                await unitOfWork.CommitAsync(ct);
                span?.SetTag("auth.result", "success");

                RecordLogin("success", entity.CongregationId.ToString());
                SessionLog.LoginSucceeded(logger, entity.Id, entity.CongregationId);

                return new SuccessResult<TokenPairResponseDtos>(
                    new TokenPairResponseDtos
                    {
                        AccessToken = accessToken,
                        AccessTokenExpiresAt = accessExpiresAt,
                        RefreshToken = plainRefreshToken,
                        RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.LoginFailed(logger, ex, entity.Id, entity.CongregationId);
            throw;
        }
    }

    public async Task<IOperationResult> LogoutOfAllSessions(
        string email,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.logout_all");

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.user_by_email"))
        {
            entity = await userRepository.GetByEmail(email, ct);
        }

        if (entity is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            SessionLog.LogoutOfAllSessionsFailed(logger);
            return new OkResult("All active sessions for this user have been terminated.");
        }

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.revoke_all"))
            {
                await jwtTokenService.RevokeAllActiveSessionsForUserById(entity.Id, ct);
                await unitOfWork.CommitAsync(ct);
                span?.SetTag("auth.result", "success");
                RecordLogoutAll("success", entity.CongregationId.ToString());
                SessionLog.LogoutOfAllSessionsSucceeded(logger, entity.Id, entity.CongregationId);

                return new OkResult("All active sessions for this user have been terminated.");
            }
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.LogoutAllFailed(logger, ex, entity.Id, entity.CongregationId);
            throw;
        }
    }

    public async Task<IOperationResult> RefreshAccessToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.refresh");

        RefreshTokenValidationResult result;
        using (instrumentation.ActivitySource.StartActivity("svc.session.validate.refresh_token"))
        {
            result = await jwtTokenService.ValidateAndRotateAsync(refreshToken, ct);
        }

        if (!result.Success)
        {
            var reason = result.FailureReason!.Value.ToString().ToLowerInvariant();
            span?.SetTag("auth.result", reason);
            RecordTokenRefresh("failure", "unknown");
            SessionLog.TokenRefreshFailed(logger, reason);
            return new BadRequestResult(MapFailureReason(result.FailureReason!.Value));
        }

        User? entity;
        using (instrumentation.ActivitySource.StartActivity("svc.session.fetch.user_by_id"))
        {
            entity = await userRepository.GetById(result.UserId, ct);
        }

        if (entity is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            RecordTokenRefresh("failure", "unknown");
            SessionLog.TokenRefreshUserNotFound(logger, result.UserId);
            return new InternalServerError();
        }

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.tokens"))
            {
                var userDto = mapper.ToAuthDto(entity);
                var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(userDto);
                await unitOfWork.CommitAsync(ct);
                span?.SetTag("auth.result", "success");
                RecordTokenRefresh("success", entity.CongregationId.ToString());
                SessionLog.TokenRefreshSucceeded(logger, entity.Id, entity.CongregationId);

                return new SuccessResult<TokenPairResponseDtos>(
                    new TokenPairResponseDtos
                    {
                        AccessToken = accessToken,
                        AccessTokenExpiresAt = accessExpiresAt,
                        RefreshToken = result.NewRefreshToken!,
                        RefreshTokenExpiresAt = result.NewRefreshTokenExpiresAt!.Value,
                    }
                );
            }
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.TokenRefreshFailed(logger, ex, entity.Id, entity.CongregationId);
            throw;
        }
    }

    public async Task<IOperationResult> RevokeAccessToken(
        string refreshToken,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.revoke");

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.revoke_token"))
            {
                await jwtTokenService.RevokeToken(refreshToken, ct);
                await unitOfWork.CommitAsync(ct);
            }

            span?.SetTag("auth.result", "success");
            RecordTokenRevocation("success");
            SessionLog.TokenRevoked(logger);

            return new OkResult("Token revoked successfully.");
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.TokenRevocationFailed(logger, ex);
            throw;
        }
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

    private void RecordLogin(string result, string congregationId) =>
        instrumentation.LoginAttempts.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );

    private void RecordTokenRefresh(string result, string congregationId) =>
        instrumentation.TokenRefreshes.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );

    private void RecordTokenRevocation(string result) =>
        instrumentation.TokenRevocations.Add(1, new TagList { { "auth.result", result } });

    private void RecordLogoutAll(string result, string congregationId) =>
        instrumentation.SessionLogouts.Add(
            1,
            new TagList { { "auth.result", result }, { "congregation.id", congregationId } }
        );
}
