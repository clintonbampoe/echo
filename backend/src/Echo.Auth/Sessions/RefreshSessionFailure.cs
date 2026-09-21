namespace Echo.Auth.Sessions;

public enum RefreshSessionFailure
{
    NotFound,
    Expired,
    Reused,
    UserInactive
}
