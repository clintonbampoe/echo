namespace Echo.Auth.Sessions;

public enum RefreshSessionFailure
{
    NotFound,
    Expired,
    Reused,

    /// <summary>Rotated a few seconds ago: most likely a second tab racing the first.</summary>
    Superseded,
    UserInactive
}
