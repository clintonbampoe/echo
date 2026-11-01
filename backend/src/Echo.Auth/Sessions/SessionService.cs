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
    // A valid bcrypt hash (cost 10) of a throwaway string; nobody's password hashes to it.
    private const string DummyHash = "$2a$10$uAV3pq9RPnRtPbczFax2euG34T7Bf0qq6qO2Gfc9rgww4W7VqVHD6";

    public async Task<IOperationResult> Login(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.login");

        User? entity;
        entity = await userRepository.GetByEmail(email, ct);

        if (entity is null)
        {
            // Do the same hashing work as for a real account, so response time does not tell an
            // attacker which email addresses are registered.
            await hashService.VerifyAsync(password, DummyHash);

            RecordLogin(span, "user_not_found", "unknown");
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
                RecordLogin(span, "invalid_credentials", entity.CongregationId.ToString());
                SessionLog.LoginInvalidCredentials(logger, entity.Id);
                return new BadRequestResult("Email or password is invalid.");
            }
        }

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.tokens"))
            {
                var tokens = await StartSession(entity, ct);
                await unitOfWork.CommitAsync(ct);
                RecordLogin(span, "success", entity.CongregationId.ToString());
                SessionLog.LoginSucceeded(logger, entity.Id, entity.CongregationId);

                return new SuccessResult<TokenPairResponseDtos>(tokens);
            }
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.LoginFailed(logger, ex, entity.Id, entity.CongregationId);
            throw;
        }
    }

    /// <summary>
    /// Builds a session for a user: an access token and a new refresh token. The refresh token is
    /// only added to the context; the caller's own CommitAsync saves it, so a signup stores the
    /// user and the session together. Pass <paramref name="prune"/> false for a brand-new user,
    /// who has no old tokens to clear.
    /// </summary>
    public async Task<TokenPairResponseDtos> StartSession(
        User user,
        CancellationToken ct,
        bool prune = true
    )
    {
        var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(mapper.ToAuthDto(user));
        var (refreshTokenEntity, plainRefreshToken) = await jwtTokenService.IssueToken(
            user.Id,
            ct,
            prune
        );

        return new TokenPairResponseDtos
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = plainRefreshToken,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
        };
    }

    public async Task<IOperationResult> LogoutOfAllSessions(
        string email,
        CancellationToken ct = default
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.logout_all");

        User? entity;
        entity = await userRepository.GetByEmail(email, ct);

        if (entity is null)
        {
            span?.SetTag("auth.result", "user_not_found");
            SessionLog.LogoutOfAllSessionsFailed(logger);
            return new SuccessResult<string>(
                "All active sessions for this user have been terminated."
            );
        }

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.revoke_all"))
            {
                await jwtTokenService.RevokeAllActiveSessionsForUserById(entity.Id, ct);
                await unitOfWork.CommitAsync(ct);
                RecordLogoutAll(span, "success", entity.CongregationId.ToString());
                SessionLog.LogoutOfAllSessionsSucceeded(logger, entity.Id, entity.CongregationId);

                return new SuccessResult<string>(
                    "All active sessions for this user have been terminated."
                );
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
            RecordTokenRefresh(null, "failure", "unknown");
            SessionLog.TokenRefreshFailed(logger, reason);
            return new BadRequestResult(MapFailureReason(result.FailureReason!.Value));
        }

        // The token lookup already loaded the user.
        var entity = result.User!;

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        try
        {
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.tokens"))
            {
                var userDto = mapper.ToAuthDto(entity);
                var (accessToken, accessExpiresAt) = accessTokenGenerator.Generate(userDto);
                await unitOfWork.CommitAsync(ct);
                RecordTokenRefresh(span, "success", entity.CongregationId.ToString());
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

            RecordTokenRevocation(span, "success");
            SessionLog.TokenRevoked(logger);

            return new SuccessResult<string>("Token revoked successfully.");
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
            RefreshSessionFailure.Superseded =>
                "Your session was just refreshed elsewhere. Please try again.",
            RefreshSessionFailure.UserInactive => "This account is no longer active.",
            _ => "Please log in again.",
        };

    private void RecordTokenRefresh(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.TokenRefreshes, span, result, congregationId);

    private void RecordTokenRevocation(Activity? span, string result) =>
        instrumentation.Record(instrumentation.TokenRevocations, span, result);

    private void RecordLogoutAll(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.SessionLogouts, span, result, congregationId);

    private void RecordLogin(Activity? span, string result, string congregationId) =>
        instrumentation.Record(instrumentation.LoginAttempts, span, result, congregationId);
}
