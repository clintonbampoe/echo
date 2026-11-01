using Echo.Application.Users;
using Echo.Data;
using Echo.Domain.Users;
using Echo.IntegrationTests.Support;

namespace Echo.IntegrationTests.Support;

[Trait("Category", "Integration")]
[Collection(PostgresCollection.Name)]
public class FixtureSmokeTests(PostgresFixture db)
{
    [Fact]
    public async Task CanCreateUserAndReadItBack()
    {
        await using var host = await AuthTestHost.CreateAsync(db);
        var users = host.Get<UserRepository>();
        var unitOfWork = host.Get<IUnitOfWork>();

        var congregationId = await host.SeedCongregationAsync();
        users.Create(
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ama",
                LastName = "Mensah",
                EmailAddress = "ama@echo.test",
                PasswordHash = "x",
                Role = UserRole.Admin,
                CongregationId = congregationId,
            }
        );
        host.Commands.Reset();
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        var found = await users.GetByEmail("ama@echo.test", TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal("ama@echo.test", found.EmailAddress);
        Assert.True(host.Commands.Count > 0);
    }
}
