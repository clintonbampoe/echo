using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Echo.Auth;

public sealed class AuthInstrumentation : IDisposable
{
    public const string SourceName = "Echo.Auth";

    private static readonly string? _version = typeof(AuthInstrumentation)
        .Assembly.GetName()
        .Version?.ToString();

    public ActivitySource ActivitySource { get; } = new(SourceName, _version);

    private readonly Meter _meter = new(SourceName, _version);

    public Counter<long> LoginAttempts { get; }
    public Counter<long> SessionLogouts { get; }
    public Counter<long> TokenRefreshes { get; }
    public Counter<long> TokenRevocations { get; }
    public Counter<long> PasswordResetRequests { get; }
    public Counter<long> PasswordResets { get; }
    public Counter<long> CongregationRegistrations { get; }
    public Counter<long> EmailVerificationsSent { get; }
    public Counter<long> EmailVerificationsCompleted { get; }
    public Counter<long> UserRegistrations { get; }
    public Counter<long> InvitationsCreated { get; }

    public AuthInstrumentation()
    {
        // SESSIONS
        LoginAttempts = _meter.CreateCounter<long>(
            "auth.login.attempts",
            unit: "{attempt}",
            description: "Number of login attempts by request and congregation"
        );

        TokenRefreshes = _meter.CreateCounter<long>(
            "auth.token.refresh",
            unit: "{refresh}",
            description: "Number of token refresh attempts by request and congregation"
        );

        TokenRevocations = _meter.CreateCounter<long>(
            "auth.token.revoke",
            unit: "{revoke}",
            description: "Number of token revocations by request and congregation"
        );

        SessionLogouts = _meter.CreateCounter<long>(
            "auth.session.logout_all",
            unit: "{logout}",
            description: "Number of logout-all-sessions operations by result and congregation"
        );

        // PASSWORDS
        PasswordResetRequests = _meter.CreateCounter<long>(
            "auth.password.reset_requested",
            unit: "{request}",
            description: "Number of password reset link requests by result and congregation"
        );

        PasswordResets = _meter.CreateCounter<long>(
            "auth.password.reset_completed",
            unit: "{reset}",
            description: "Number of password reset completions by result and congregation"
        );

        // REGISTRATIONS
        CongregationRegistrations = _meter.CreateCounter<long>(
            "auth.registration.congregation",
            unit: "{registration}",
            description: "Number of congregation registration attempts by result"
        );

        UserRegistrations = _meter.CreateCounter<long>(
            "auth.registration.user",
            unit: "{registration}",
            description: "Number of user registration attempts by result and congregation"
        );

        // EMAIL EmailVerifications
        EmailVerificationsSent = _meter.CreateCounter<long>(
            "auth.email_verification.sent",
            unit: "{verification}",
            description: "Number of verification email send attempts by result and congregation"
        );

        EmailVerificationsCompleted = _meter.CreateCounter<long>(
            "auth.email_verification.completed",
            unit: "{verification}",
            description: "Number of email verification completions by result and congregation"
        );

        // INVITATIONS
        InvitationsCreated = _meter.CreateCounter<long>(
            "auth.invitation.created",
            unit: "{invitation}",
            description: "Number of invitation tokens created by congregation and allowed role"
        );
    }

    public static TracerProviderBuilder ConfigureTracing(TracerProviderBuilder tracing) =>
        tracing.AddSource(SourceName);

    public static MeterProviderBuilder ConfigureMetrics(MeterProviderBuilder metrics) =>
        metrics.AddMeter(SourceName);

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}
