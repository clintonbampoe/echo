using Echo.Application.Users;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;

namespace Echo.IntegrationTests.Auth;

/// <summary>The <c>q</c> search of the user list, which needs a real database for ILIKE.</summary>
[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class UserListSearchTests(PostgresFixture db)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static User Person(Guid church, string name, string email, UserRole role)
    {
        var user = TestHostExtensions.NewUser(church, email);
        user.Name = name;
        user.Role = role;
        return user;
    }

    private async Task<(AuthTestHost Host, Guid Church)> Arrange()
    {
        var host = await AuthTestHost.CreateAsync(db);
        var church = await host.SeedCongregationAsync();
        var repository = host.Get<UserRepository>();
        repository.Create(Person(church, "Ama Mensah", "ama@echo.test", UserRole.Clerk));
        repository.Create(Person(church, "Kofi Owusu", "kofi.owusu@echo.test", UserRole.Clerk));
        repository.Create(Person(church, "Efua Annan", "efua@echo.test", UserRole.Member));
        await host.Get<IUnitOfWork>().CommitAsync(Ct);
        return (host, church);
    }

    private static async Task<List<string>> Names(AuthTestHost host, Guid church, UserFilter filter) =>
        (await host.Get<UserRepository>().List(church, filter, null, 50, Ct)).Select(u => u.Name).ToList();

    [Fact]
    public async Task Q_MatchesNameOrEmail_IgnoringCase()
    {
        var (host, church) = await Arrange();
        await using var _ = host;

        Assert.Equal(["Kofi Owusu"], await Names(host, church, new UserFilter(Q: "OWUSU")));
        Assert.Equal(["Efua Annan"], await Names(host, church, new UserFilter(Q: "efua@")));
    }

    [Fact]
    public async Task Q_CombinesWithRole()
    {
        var (host, church) = await Arrange();
        await using var _ = host;

        // Only Ama is a Clerk whose name or email starts with "am".
        Assert.Equal(["Ama Mensah"], await Names(host, church, new UserFilter(UserRole.Clerk, Q: "am")));
        Assert.Empty(await Names(host, church, new UserFilter(UserRole.Member, Q: "ama")));
    }

    [Fact]
    public async Task Q_TreatsWildcardsAsText()
    {
        var (host, church) = await Arrange();
        await using var _ = host;

        Assert.Empty(await Names(host, church, new UserFilter(Q: "%%%")));
    }

    [Fact]
    public async Task Q_ShortText_MatchesOnlyTheStart()
    {
        var (host, church) = await Arrange();
        await using var _ = host;

        Assert.Equal(["Kofi Owusu"], await Names(host, church, new UserFilter(Q: "ko")));
        Assert.Empty(await Names(host, church, new UserFilter(Q: "fi")));
    }

    [Fact]
    public async Task BlankQ_ReturnsEveryone()
    {
        var (host, church) = await Arrange();
        await using var _ = host;

        Assert.Equal(3, (await Names(host, church, new UserFilter(Q: "  "))).Count);
    }
}
