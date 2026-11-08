using Echo.Application.Users;
using Echo.Application.Users.Emails;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;

namespace Echo.IntegrationTests.Auth;

/// <summary>The emails a person gets when an admin changes their role, disables them or removes them.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class AccountChangeEmailTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, Guid Church, User Admin, User Person)> Arrange(
        UserRole personRole
    )
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var church = await host.SeedCongregationAsync();

        var admin = TestHostExtensions.NewUser(church, "kwabena@echo.test");
        admin.Name = "Kwabena Ansah";
        var person = TestHostExtensions.NewUser(church, "ama@echo.test");
        person.Role = personRole;
        host.Get<UserRepository>().Create(admin);
        host.Get<UserRepository>().Create(person);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, church, admin, person);
    }

    [Fact]
    public async Task ChangingARole_EmailsThePerson_WithWhoChangedIt()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Accountant);
        await using var _ = host;

        await host.Get<UserService>().Update(church, admin.Id, person.Id, new UserUpdateDto { Role = UserRole.Admin }, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("ama@echo.test", message.To);
        Assert.Equal("role_changed", message.Kind);
        Assert.IsType<RoleChangedEmailBody>(message.Content);
        Assert.Contains("Kwabena Ansah", message.Content.HtmlBody);
        Assert.StartsWith("You are now an Admin", message.Content.Subject);
    }

    [Fact]
    public async Task LoweringARole_SaysSo()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Admin);
        await using var _ = host;

        await host.Get<UserService>().Update(church, admin.Id, person.Id, new UserUpdateDto { Role = UserRole.Clerk }, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Contains("changed to Clerk", message.Content.Subject);
    }

    [Fact]
    public async Task UpdateWithoutARoleChange_SendsNothing()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Clerk);
        await using var _ = host;
        var service = host.Get<UserService>();

        await service.Update(church, admin.Id, person.Id, new UserUpdateDto { Role = UserRole.Clerk }, Ct);
        await service.Update(church, admin.Id, person.Id, new UserUpdateDto(), Ct);

        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task Disabling_EmailsThePerson_OnlyTheFirstTime()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Member);
        await using var _ = host;
        var service = host.Get<UserService>();

        await service.Disable(church, admin.Id, person.Id, Ct);
        await service.Disable(church, admin.Id, person.Id, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("account_disabled", message.Kind);
        Assert.IsType<AccountDisabledEmailBody>(message.Content);
    }

    [Fact]
    public async Task Removing_EmailsThePerson()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Member);
        await using var _ = host;

        await host.Get<UserService>().Delete(church, admin.Id, person.Id, Ct);

        var message = Assert.Single(queue.Messages);
        Assert.Equal("account_removed", message.Kind);
        Assert.IsType<AccountRemovedEmailBody>(message.Content);
    }

    [Fact]
    public async Task ARefusedChange_SendsNothing()
    {
        var (host, queue, church, admin, _) = await Arrange(UserRole.Member);
        await using var _ = host;

        // An admin cannot disable themselves.
        await host.Get<UserService>().Disable(church, admin.Id, admin.Id, Ct);

        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task AFullQueue_DoesNotUndoTheChange()
    {
        var (host, queue, church, admin, person) = await Arrange(UserRole.Member);
        await using var _ = host;
        queue.Accepting = false;

        var result = await host.Get<UserService>().Disable(church, admin.Id, person.Id, Ct);

        Assert.NotNull(result.Value<UserResponseDto>().DisabledAt);
    }
}
