using Echo.Application.Users;
using Echo.Domain.Users;

namespace Echo.Application.Tests.Users;

[Trait("Category", "Unit")]
public class UserFilterTests
{
    private static User NewUser(UserRole role, bool disabled = false, bool verified = true)
    {
        var user = TestData.Shared.CreateUser();
        user.Role = role;
        user.DisabledAt = disabled ? DateTime.UtcNow : null;
        user.EmailVerifiedAt = verified ? DateTime.UtcNow : null;
        return user;
    }

    private static readonly User[] People =
    [
        NewUser(UserRole.Admin),
        NewUser(UserRole.Clerk),
        NewUser(UserRole.Clerk, disabled: true),
        NewUser(UserRole.Member, verified: false),
    ];

    private static List<User> Apply(UserFilter filter) =>
        People.AsQueryable().Filter(filter).ToList();

    [Fact]
    public void NoFilter_ReturnsEveryone() => Assert.Equal(4, Apply(new UserFilter()).Count);

    [Fact]
    public void Role_ReturnsOnlyThatRole() =>
        Assert.All(Apply(new UserFilter(Role: UserRole.Clerk)), u => Assert.Equal(UserRole.Clerk, u.Role));

    [Fact]
    public void Status_Active_LeavesOutDisabled() =>
        Assert.Equal(3, Apply(new UserFilter(Status: UserStatus.Active)).Count);

    [Fact]
    public void Status_Disabled_ReturnsOnlyDisabled() =>
        Assert.Single(Apply(new UserFilter(Status: UserStatus.Disabled)));

    [Fact]
    public void Status_Unverified_ReturnsOnlyUnverified() =>
        Assert.Single(Apply(new UserFilter(Status: UserStatus.Unverified)));

    [Fact]
    public void RoleAndStatus_Combine() =>
        Assert.Single(Apply(new UserFilter(UserRole.Clerk, UserStatus.Disabled)));
}
