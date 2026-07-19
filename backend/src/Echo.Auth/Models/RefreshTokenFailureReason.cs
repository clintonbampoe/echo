namespace Echo.Auth.Models;

public enum RefreshTokenFailureReason
{
    NotFound,
    Expired,
    Reused,
    UserInactive
}
