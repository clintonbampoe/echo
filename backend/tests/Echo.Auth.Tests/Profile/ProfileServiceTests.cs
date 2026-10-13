using Echo.Auth.Profile;
using Echo.Domain.Users;

namespace Echo.Auth.Tests.Profile;

[Trait("Category", "Unit")]
public class ProfileServiceTests
{
    [Fact]
    public void Validate_NoPasswordFields_IsValid()
    {
        Assert.Null(ProfileService.Validate(new ProfileUpdateDto { FirstName = "Ama" }));
    }

    [Fact]
    public void Validate_NewPasswordWithoutCurrent_AsksForBoth()
    {
        var error = ProfileService.Validate(new ProfileUpdateDto { NewPassword = "longenough1" });

        Assert.Contains("currentPassword", error);
    }

    [Fact]
    public void Validate_CurrentPasswordWithoutNew_AsksForBoth()
    {
        var error = ProfileService.Validate(new ProfileUpdateDto { CurrentPassword = "whatever" });

        Assert.Contains("newPassword", error);
    }

    [Fact]
    public void Validate_NewPasswordBreakingThePolicy_ReportsThePolicyError()
    {
        var error = ProfileService.Validate(
            new ProfileUpdateDto { CurrentPassword = "whatever", NewPassword = "short" }
        );

        Assert.Contains("at least 8", error);
    }

    [Fact]
    public void Validate_BothPasswordsAndAValidPolicy_IsValid()
    {
        Assert.Null(
            ProfileService.Validate(
                new ProfileUpdateDto { CurrentPassword = "whatever", NewPassword = "longenough1" }
            )
        );
    }

    [Fact]
    public void Validate_ABlankName_IsRejected()
    {
        var error = ProfileService.Validate(new ProfileUpdateDto { FirstName = "   " });

        Assert.Contains("cannot be blank", error);
    }

    [Fact]
    public void Apply_ChangesOnlyTheNamesThatWereSent_AndTrimsThem()
    {
        var user = new User { FirstName = "Ama", LastName = "Mensah", OtherNames = "K" };

        ProfileService.Apply(user, new ProfileUpdateDto { FirstName = "  Abena  " });

        Assert.Equal(("Abena", "Mensah", "K"), (user.FirstName, user.LastName, user.OtherNames));
    }

    [Fact]
    public void Apply_NeverTouchesTheRoleOrEmail()
    {
        var user = new User { Role = UserRole.Member, EmailAddress = "ama@example.com" };

        ProfileService.Apply(user, new ProfileUpdateDto { FirstName = "Abena", LastName = "Mensah" });

        Assert.Equal((UserRole.Member, "ama@example.com"), (user.Role, user.EmailAddress));
    }
}
