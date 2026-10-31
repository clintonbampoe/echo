namespace Echo.Auth;

internal static class TokenValidity
{
    /// <summary>
    /// Why a one-time token (email verification, password reset) cannot be used, as the
    /// <c>auth.result</c> value: <c>token_expired</c>, <c>token_used</c> or
    /// <c>token_invalidated</c> (checked in that order). Null when it is still usable.
    /// </summary>
    internal static string? Problem(
        DateTime expiresAt,
        DateTime? usedAt,
        DateTime? invalidatedAt,
        DateTime now
    )
    {
        if (expiresAt <= now)
            return "token_expired";
        if (usedAt is not null)
            return "token_used";
        if (invalidatedAt is not null)
            return "token_invalidated";
        return null;
    }
}
