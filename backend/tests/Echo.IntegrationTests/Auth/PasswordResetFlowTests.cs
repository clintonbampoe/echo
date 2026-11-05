using System.Text.RegularExpressions;
using Echo.Application.Users;
using Echo.Auth.Passwords;
using Echo.Data;
using Echo.Domain.Auth;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;
using Echo.Shared.HttpResults;
using Echo.Shared.Services.Hashing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class PasswordResetFlowTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string Sent = "Reset link has been sent.";

    private async Task<(AuthTestHost Host, RecordingEmailQueue Queue, User User)> Arrange()
    {
        var queue = new RecordingEmailQueue();
        var host = await AuthTestHost.CreateAsync(db, TestHostExtensions.WithRecordedEmail(queue));
        var congregationId = await host.SeedCongregationAsync();
        var user = TestHostExtensions.NewUser(congregationId);
        host.Get<UserRepository>().Create(user);
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, queue, user);
    }

    private static string TokenFromEmail(Echo.Shared.Services.Email.EmailMessage message) =>
        Uri.UnescapeDataString(Regex.Match(message.Content.HtmlBody, @"token=([^""&<\s]+)").Groups[1].Value);

    private static async Task<List<PasswordResetToken>> TokensOf(AuthTestHost host, Guid userId)
    {
        using var scope = host.NewScope();
        return await scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Set<PasswordResetToken>()
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(Ct);
    }

    [Fact]
    public async Task Forgot_UsesThreeStatements_UserWithLinks_Prune_Save()
    {
        var (host, _, user) = await Arrange();
        await using var _host = host;
        host.Commands.Reset();

        await host.Get<PasswordResetService>().SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);

        Assert.Equal(3, host.Commands.Count);
    }

    [Fact]
    public async Task Forgot_Twice_InsideSixtySeconds_QueuesOneEmail_BothReturnTheSameMessage()
    {
        var (host, queue, user) = await Arrange();
        await using var _ = host;
        var service = host.Get<PasswordResetService>();

        var first = await service.SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);
        host.Time.Advance(TimeSpan.FromSeconds(30));
        var second = await service.SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);

        Assert.Single(queue.Messages);
        Assert.Equal(Sent, first.Message());
        Assert.Equal(Sent, second.Message());
        Assert.Single(await TokensOf(host, user.Id));
    }

    [Fact]
    public async Task Forgot_AfterSixtySeconds_InvalidatesTheOldLink()
    {
        var (host, queue, user) = await Arrange();
        await using var _host = host;
        var service = host.Get<PasswordResetService>();
        await service.SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);
        var oldToken = TokenFromEmail(queue.Messages[0]);
        host.Time.Advance(TimeSpan.FromSeconds(61));

        await service.SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);

        Assert.Equal(2, queue.Messages.Count);
        var oldLink = await service.ResetPassword(oldToken, "brand new password", Ct);
        Assert.IsType<InvalidTokenResult>(oldLink);
        var newLink = await service.ResetPassword(TokenFromEmail(queue.Messages[1]), "brand new password", Ct);
        Assert.IsNotType<InvalidTokenResult>(newLink);
    }

    [Fact]
    public async Task ResetPassword_QueuesAPasswordChangedEmail()
    {
        var (host, queue, user) = await Arrange();
        await using var _host = host;
        var service = host.Get<PasswordResetService>();
        await service.SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);

        await service.ResetPassword(TokenFromEmail(queue.Messages[0]), "brand new password", Ct);

        Assert.Equal(2, queue.Messages.Count);
        var changed = queue.Messages[1];
        Assert.Equal("password_changed", changed.Kind);
        Assert.Equal(user.EmailAddress, changed.To);
        Assert.Equal(user.Id, changed.UserId);
    }

    [Fact]
    public async Task ResetPassword_WithABadToken_QueuesNoPasswordChangedEmail()
    {
        var (host, queue, _) = await Arrange();
        await using var _host = host;

        await host.Get<PasswordResetService>().ResetPassword("no-such-token", "brand new password", Ct);

        Assert.Empty(queue.Messages);
    }

    [Fact]
    public async Task Forgot_UnknownEmail_SameMessage_NothingQueued()
    {
        var (host, queue, _) = await Arrange();
        await using var _host = host;

        var result = await host.Get<PasswordResetService>().SendForgotPasswordLinkToEmail("nobody@echo.test", Ct);

        Assert.Empty(queue.Messages);
        Assert.Equal(Sent, result.Message());
    }

    [Fact]
    public async Task ResetPassword_InvalidatedToken_IsRejected()
    {
        var (host, _, user) = await Arrange();
        await using var _host = host;
        var hash = host.Get<ITokenHasher>().Hash("invalidated-token");
        host.Get<AppDbContext>().Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = host.Time.GetUtcNow().UtcDateTime.AddHours(1),
            InvalidatedAt = host.Time.GetUtcNow().UtcDateTime,
        });
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        var result = await host.Get<PasswordResetService>().ResetPassword("invalidated-token", "brand new password", Ct);

        Assert.IsType<InvalidTokenResult>(result);
    }

    [Fact]
    public async Task Forgot_PrunesOldTokensOfTheUser()
    {
        var (host, _, user) = await Arrange();
        await using var _host = host;
        var now = host.Time.GetUtcNow().UtcDateTime;
        var context = host.Get<AppDbContext>();
        context.AddRange(
            new PasswordResetToken { UserId = user.Id, TokenHash = "expired", ExpiresAt = now.AddHours(-2) },
            new PasswordResetToken { UserId = user.Id, TokenHash = "used", ExpiresAt = now.AddHours(1), UsedAt = now.AddMinutes(-5) },
            new PasswordResetToken { UserId = user.Id, TokenHash = "old-invalidated", ExpiresAt = now.AddHours(1), InvalidatedAt = now.AddDays(-8) }
        );
        await host.Get<IUnitOfWork>().CommitAsync(Ct);

        await host.Get<PasswordResetService>().SendForgotPasswordLinkToEmail(user.EmailAddress, Ct);

        var left = await TokensOf(host, user.Id);
        Assert.Single(left);
        Assert.DoesNotContain(left, t => t.TokenHash is "expired" or "used" or "old-invalidated");
    }
}
