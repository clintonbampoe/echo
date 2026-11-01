namespace Echo.Auth.Tests;

[Trait("Category", "Unit")]
public class TokenValidityTests
{
    private static readonly DateTime Now = new(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Problem_Expired_ReturnsTokenExpired() =>
        Assert.Equal("token_expired", TokenValidity.Problem(Now.AddSeconds(-1), null, null, Now));

    [Fact]
    public void Problem_ExpiringExactlyNow_CountsAsExpired() =>
        Assert.Equal("token_expired", TokenValidity.Problem(Now, null, null, Now));

    [Fact]
    public void Problem_Used_ReturnsTokenUsed() =>
        Assert.Equal(
            "token_used",
            TokenValidity.Problem(Now.AddHours(1), Now.AddMinutes(-1), null, Now)
        );

    [Fact]
    public void Problem_Invalidated_ReturnsTokenInvalidated() =>
        Assert.Equal(
            "token_invalidated",
            TokenValidity.Problem(Now.AddHours(1), null, Now.AddMinutes(-1), Now)
        );

    [Fact]
    public void Problem_ExpiredAndUsed_ReturnsExpiredFirst() =>
        Assert.Equal(
            "token_expired",
            TokenValidity.Problem(Now.AddHours(-1), Now.AddHours(-2), null, Now)
        );

    [Fact]
    public void Problem_UsedAndInvalidated_ReturnsUsedFirst() =>
        Assert.Equal(
            "token_used",
            TokenValidity.Problem(Now.AddHours(1), Now.AddMinutes(-2), Now.AddMinutes(-1), Now)
        );

    [Fact]
    public void Problem_Valid_ReturnsNull() =>
        Assert.Null(TokenValidity.Problem(Now.AddHours(1), null, null, Now));
}
