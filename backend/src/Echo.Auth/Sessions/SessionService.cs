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
    // A valid Argon2id hash (the same settings as real ones) of a throwaway string; nobody's password hashes to it.
    private const string DummyHash = "$argon2id$v=19$m=19456,t=2,p=1$azKDxfBAGiQdtl1SqDYPxQ$7YJ87MxLdkExeog/dQ7L+zZSRVB/yxzmdT/5b1CegcU";

    /// <summary>
    /// When many people sign in at once the password checks queue up (see HashGate) and a caller who would wait too long
    /// is told to try again. That is answered here as a plain 503, not left to the error handler: it is expected under
    /// load, and an exception per refused caller would cost log lines and stack traces exactly when the CPU is short.
    /// </summary>
    public async Task<IOperationResult> Login(
        string email,
        string password,
        CancellationToken ct = default
    )
    {
        try
        {
            return await Authenticate(email, password, ct);
        }
        catch (HashingBusyException busy)
        {
            RecordLogin(Activity.Current, "busy", "unknown");
            return new ServiceBusyResult(busy.RetryAfter);
        }
    }

    private async Task<IOperationResult> Authenticate(
        string email,
        string password,
        CancellationToken ct
    )
    {
        using var span = instrumentation.ActivitySource.StartActivity("svc.session.login");

        User? entity;
        entity = await userRepository.GetByEmail(email, ct);

        if (entity is null)
        {
            // Do the same hashing work as for a real account, so response time does not tell an
            // attacker which email addresses are registered.
            await hashService.VerifyAsync(password, DummyHash, ct);

            RecordLogin(span, "user_not_found", "unknown");
            SessionLog.LoginUserNotFound(logger);
            return new BadRequestResult("Email or password is invalid.");
        }

        span?.SetTag("congregation.id", entity.CongregationId);
        span?.SetTag("user.id", entity.Id);

        using (instrumentation.ActivitySource.StartActivity("svc.session.validate.password"))
        {
            var isPasswordValid = await hashService.VerifyAsync(password, entity.PasswordHash, ct);
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

        try
        {
            int revoked;
            using (instrumentation.ActivitySource.StartActivity("svc.session.persist.revoke_all"))
            {
                // One statement: the same answer whether or not the email has an account.
                revoked = await jwtTokenService.RevokeAllActiveSessionsForEmail(email, ct);
            }

            RecordLogoutAll(span, "success", "unknown");
            SessionLog.LogoutOfAllSessionsCompleted(logger, revoked);

            return new SuccessResult<string>(
                "All active sessions for this user have been terminated."
            );
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SessionLog.LogoutAllFailed(logger, ex);
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
                // One statement; there is nothing for a save to do.
                await jwtTokenService.RevokeToken(refreshToken, ct);
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
